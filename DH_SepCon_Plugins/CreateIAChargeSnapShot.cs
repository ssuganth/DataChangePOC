using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DH_SepCon_Plugins
{
    public class CreateIAChargeSnapShot: IPlugin
    {
        IOrganizationService _service;
        IPluginExecutionContext _context;
        public void Execute(IServiceProvider serviceProvider)
        {
            _context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));
            if (_context.InputParameters.Contains("Target") &&
                        _context.InputParameters["Target"] is Entity)
            {
                // Obtain the target entity from the input parameters.  
                Entity entity = (Entity)_context.InputParameters["Target"];

                // Obtain the organization service reference which you will need for  
                // web service calls.  
                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                _service = serviceFactory.CreateOrganizationService(_context.UserId);
                try
                {
                    //Fetch all the DF related to the client which has ChargesApproved = Yes
                    var fetchDF = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
                                    "<entity name='ssg_violationreport'>" +
                                    "<attribute name='ssg_reportid' />" +
                                    "<attribute name='createdon' />" +
                                    "<attribute name='ssg_violationreportid' />" +
                                    "<attribute name='ssg_regualtionsectiondetailupdated' />" +
                                    "<attribute name='ssg_regulationsection' />" +
                                    "<attribute name='ssg_client' />" +
                                    "<order attribute='ssg_reportid' descending='false' />" +
                                    "<filter type='and'>" +
                                    "<condition attribute='ssg_client' operator='eq' value='" + ((EntityReference)entity.Attributes["ssg_contactid"]).Id.ToString() + "'/>" +
                                    "<condition attribute='ssg_chargesapproved' operator='eq' value='867670000'/>" +
                                    "</filter>" +
                                    "</entity>" +
                                    "</fetch>";

                    EntityCollection ecDF = _service.RetrieveMultiple(new FetchExpression(fetchDF));
                    if (ecDF.Entities.Count() > 0)
                    {

                        Dictionary<String, Int32> dcIASS = new Dictionary<string, int>();
                        //Loop through DF to get the count of Regulations
                        foreach (var DF in ecDF.Entities)
                        {
                            var sType = DF.FormattedValues["ssg_regulationsection"].ToString() + DF.FormattedValues["ssg_regualtionsectiondetailupdated"].ToString();
                            if (!dcIASS.ContainsKey(sType))
                            {
                                dcIASS.Add(sType, 1);
                            }
                            else
                            {
                                dcIASS[sType] = dcIASS[sType] + 1;

                            }
                        }

                        //Create Charge SnapShot
                        foreach (var item in dcIASS)
                        {
                            Entity enIASnapShot = new Entity("ssg_iainternalchargesnapshot");
                            enIASnapShot.Attributes.Add("ssg_type", item.Key);
                            enIASnapShot.Attributes.Add("ssg_number", item.Value);
                            enIASnapShot.Attributes.Add("ssg_source", new OptionSetValue(867670000));
                            enIASnapShot.Attributes.Add("ssg_inmateassessmentid", new EntityReference(entity.LogicalName, entity.Id));
                            _service.Create(enIASnapShot);
                        }


                    }
                }
                catch (Exception e)
                {
                    throw new InvalidPluginExecutionException(e + "PostCreateInmateAssessment Plugin error");
                }

            }
        }
    }
}
