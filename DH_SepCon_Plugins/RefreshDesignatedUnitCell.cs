using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using DH_SepCon_Plugins.Helper;
using Microsoft.Xrm.Sdk.Messages;
using System.Threading;

namespace DH_SepCon_Plugins
{
    public class RefreshDesignatedUnitCell : IPlugin
    {
        IOrganizationService _service;
        IPluginExecutionContext _context;
       
        ITracingService trace;
        String _sPrevDTFilter = String.Empty;
        

        public void Execute(IServiceProvider serviceProvider)
        {
            _context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            // Obtain the organization service reference which you will need for  
            // web service calls.  
            IOrganizationServiceFactory serviceFactory =
                (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            _service = serviceFactory.CreateOrganizationService(_context.UserId);
            trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            try
            {
                //Fetch distinct BU's which are updated - To deactivate all the other Unit Cell's which are not updated in that BU
                var fetchUpdatedBU = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>" +
                                        "<entity name='ssg_cell'>" +
                                        "<attribute name='ssg_businessunit'/>" +
                                        "<order attribute='ssg_businessunit' descending='false'/>" +
                                        "<filter type='and'>" +
                                        "<condition attribute='ssg_refreshed' operator='eq' value='1'/>" +
                                        "</filter>" +
                                        "</entity>" +
                                        "</fetch>";

                EntityCollection ecUpdatedBU = _service.RetrieveMultiple(new FetchExpression(fetchUpdatedBU));
                trace.Trace("Updated BU's: " + ecUpdatedBU.Entities.Count.ToString());

                if (ecUpdatedBU.Entities.Count > 0)
                {

                    var sFilter = string.Empty;
                    foreach (var enDUC in ecUpdatedBU.Entities)
                    {
                        sFilter = "<value>" + enDUC.GetAttributeValue<EntityReference>("ssg_businessunit").Id.ToString() + "</value>";
                    }

                    //Fetch all the Unit Cell which should be deacivated - refreshed = false or null, BU= those updated and those cell without clients
                    var fetchDUCToDeactivate = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
                                                "<entity name='ssg_cell'>" +
                                                "<attribute name='ssg_cellid'/>" +
                                                "<order attribute='ssg_businessunit' descending='false'/>" +
                                                "<filter type='and'>" +
                                                "<condition attribute ='ssg_clientid' operator= 'null'/>"+
                                                   "<filter type='or'>" +
                                                "<condition attribute='ssg_refreshed' operator='eq' value='0' />" +
                                                "<condition attribute='ssg_refreshed' operator='null' />" +
                                                "</filter>" +
                                                "<condition attribute='ssg_businessunit' operator='in'>" +
                                                sFilter +
                                                "</condition>" +
                                                "</filter>" +
                                                "</entity>" +
                                                "</fetch>";

                    EntityCollection ecDUCToDeactivate = _service.RetrieveMultiple(new FetchExpression(fetchDUCToDeactivate));
                    trace.Trace("DUC's to deactivate: " + ecDUCToDeactivate.Entities.Count.ToString());
                    if (ecDUCToDeactivate.Entities.Count > 0)
                    {
                        foreach (var enUC in ecDUCToDeactivate.Entities)
                        {
                            enUC.Attributes.Add("statecode", new OptionSetValue(1)); //Inactive
                            enUC.Attributes.Add("statuscode", new OptionSetValue(2));
                            _service.Update(enUC);
                        }
                    }

                    //Fetch all the Designated Unit Cell where Refresh is marked as Yes - to update them back to No
                    var fetchRefreshDUC = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
                                       "<entity name='ssg_cell'>" +
                                       "<attribute name='ssg_cellid'/>" +
                                       "<attribute name='ssg_businessunit'/>" +
                                       "<order attribute='ssg_businessunit' descending='false'/>" +
                                       "<filter type='and'>" +
                                       "<condition attribute='ssg_refreshed' operator='eq' value='1'/>" +
                                       "</filter>" +
                                       "</entity>" +
                                       "</fetch>";

                    EntityCollection ecRefreshDUC = _service.RetrieveMultiple(new FetchExpression(fetchRefreshDUC));
                    trace.Trace("Refresh DUC: " + ecRefreshDUC.Entities.Count.ToString());

                    if (ecRefreshDUC.Entities.Count > 0)
                    {
                        foreach (var enUC in ecRefreshDUC.Entities)
                        {
                            enUC.Attributes.Add("ssg_refreshed", false); 
                            
                            _service.Update(enUC);
                        }

                    }
                }
            }
            catch(Exception e)
            {
                throw new InvalidPluginExecutionException("RefreshDesignatedUnitCell Plugin(Action) : " + e.Message);
            }
            }
    }
}
