using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DH_SepCon_Plugins
{
    public class UpdateCTOC:IPlugin
    {
        IOrganizationService _service;
        IPluginExecutionContext _context;

        public void Execute(IServiceProvider serviceProvider)
        {
            _context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            if (_context.InputParameters.Contains("Target") &&
                        _context.InputParameters["Target"] is EntityReference)
            {
                // Obtain the target entity from the input parameters.  
                EntityReference entity = (EntityReference)_context.InputParameters["Target"];

                // Obtain the organization service reference which you will need for  
                // web service calls.  
                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                _service = serviceFactory.CreateOrganizationService(_context.UserId);
                ITracingService _tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

                var eCTOC = _service.Retrieve("ssg_separateconfinementperiod", entity.Id, new ColumnSet("ssg_s17expirydatetime"));
                var dtS17Expiry = new DateTime();
                if (eCTOC.Contains("ssg_s17expirydatetime"))
                    dtS17Expiry = eCTOC.GetAttributeValue<DateTime>("ssg_s17expirydatetime");


                //Fetch all the Confinement record related to CTOC
                var fetchConfinement = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
                                "<entity name='ssg_separateconfinement'>" +
                                "<attribute name='createdon' />" +
                                "<attribute name='ssg_date' />" +
                                "<attribute name='ssg_inmateid' />" +
                                "<attribute name='ssg_number' />" +
                                "<attribute name='ssg_separateconfinementtype_t_' />" +
                                "<attribute name='ownerid' />" +
                                "<attribute name='statuscode' />" +
                                "<attribute name='ssg_separateconfinementid' />" +
                                "<attribute name='statecode' />" +
                                "<attribute name='ssg_actualenddatetime' />" +
                                 "<attribute name='ssg_s17starteddate' />" +
                                "<attribute name='ssg_separateconfinementenddate' />" +
                                "<order attribute='ssg_inmateid' descending='false' />" +
                                "<filter type='and'>" +
                                "<condition attribute='ssg_periodofseparateconfinement' operator='eq' value='" + entity.Id + "' />" +
                                "<condition attribute='statecode' value='0' operator='eq'/>" +
                                "</filter>" +
                                "</entity>" +
                                "</fetch>";

                EntityCollection ecConfinement = _service.RetrieveMultiple(new FetchExpression(fetchConfinement));
                _tracingService.Trace("Custom Workflow - UpdateCTOC : Count of Confinement " + ecConfinement.Entities.Count().ToString());
                //if there are active confinements, update CTOC accordingly
                if (ecConfinement.Entities.Count() > 0)
                {
                    //confinement type in CTOC is a comma separated value of all the active confinement types
                    var dtExpiry = new DateTime();
                    var dtCARStart = new DateTime();
                    var dtMOStartDate = new DateTime();
                    var sConfinementType = String.Empty;
                    var iConfinementDays = 0;
                    var sMOConfinementType = String.Empty;

                    foreach (var confinement in ecConfinement.Entities)
                    {
                        _tracingService.Trace("Custom Workflow - UpdateCTOC : ssg_separateconfinementtype_t_: " + confinement.GetAttributeValue<OptionSetValue>("ssg_separateconfinementtype_t_").Value.ToString());
                        //If Confinement is of CAR type
                        if (confinement.GetAttributeValue<OptionSetValue>("ssg_separateconfinementtype_t_").Value!= 867670006 && confinement.GetAttributeValue<OptionSetValue>("ssg_separateconfinementtype_t_").Value != 867670007)
                        {
                            _tracingService.Trace("Custom Workflow - UpdateCTOC : Confinement CAR Type");
                            if (dtCARStart != new DateTime() && dtCARStart > confinement.GetAttributeValue<DateTime>("ssg_date"))
                                dtCARStart = confinement.GetAttributeValue<DateTime>("ssg_date");
                            else if (dtCARStart == new DateTime())
                                dtCARStart = confinement.GetAttributeValue<DateTime>("ssg_date");

                            if ((dtS17Expiry == new DateTime() && confinement.Contains("ssg_s17starteddate")) && (confinement.GetAttributeValue<OptionSetValue>("ssg_separateconfinementtype_t_").Value == 867670004 || confinement.GetAttributeValue<OptionSetValue>("ssg_separateconfinementtype_t_").Value == 867670000))
                            {
                                _tracingService.Trace("Current Confinement type: " + confinement.GetAttributeValue<OptionSetValue>("ssg_separateconfinementtype_t_").Value.ToString());
                                TimeSpan ts = new TimeSpan(23, 59, 0);
                                dtS17Expiry = confinement.GetAttributeValue<DateTime>("ssg_s17starteddate").AddDays(3) + ts;
                            }

                        }
                        else // If Confinement is of MO Type
                        {
                            _tracingService.Trace("Custom Workflow - UpdateCTOC : Confinement MO Type");
                            if (dtMOStartDate != new DateTime() && dtMOStartDate > confinement.GetAttributeValue<DateTime>("ssg_date"))
                                dtMOStartDate = confinement.GetAttributeValue<DateTime>("ssg_date");
                            else if (dtMOStartDate == new DateTime())
                                dtMOStartDate = confinement.GetAttributeValue<DateTime>("ssg_date");
                            if (confinement.GetAttributeValue<OptionSetValue>("ssg_separateconfinementtype_t_").Value == 867670006)
                                sMOConfinementType = sMOConfinementType + "MO-IND;";
                            else
                                sMOConfinementType = sMOConfinementType + "MO-ISO;";
                        }

                        _tracingService.Trace("Custom Workflow - Confinement Type Formatted Value: " + confinement.FormattedValues["ssg_separateconfinementtype_t_"].ToString());
                        sConfinementType = sConfinementType + confinement.FormattedValues["ssg_separateconfinementtype_t_"].ToString() + ";";
                        if (dtExpiry != new DateTime() && dtExpiry < confinement.GetAttributeValue<DateTime>("ssg_separateconfinementenddate"))
                            dtExpiry = confinement.GetAttributeValue<DateTime>("ssg_separateconfinementenddate");
                        else if(dtExpiry == new DateTime())
                            dtExpiry = confinement.GetAttributeValue<DateTime>("ssg_separateconfinementenddate");

                        

                    }

                    Entity updateCTOC = new Entity("ssg_separateconfinementperiod");
                    if (dtCARStart != new DateTime() )
                    {
                        iConfinementDays = Convert.ToInt32((DateTime.Today - dtCARStart.Date).TotalDays+1);
                        updateCTOC.Attributes.Add("ssg_currentconfinementperiodstartdate", dtCARStart);
                    }
                    else if (dtCARStart == new DateTime() && dtMOStartDate != new DateTime())
                    {
                        iConfinementDays = Convert.ToInt32((DateTime.Today - dtMOStartDate.Date).TotalDays +1);
                        updateCTOC.Attributes.Add("ssg_currentconfinementperiodstartdate", dtMOStartDate);
                    }

                    if(dtS17Expiry!= new DateTime())
                        updateCTOC.Attributes.Add("ssg_s17expirydatetime", dtS17Expiry);
                    updateCTOC.Attributes.Add("ssg_covid", sMOConfinementType.TrimEnd().TrimEnd(';'));
                    updateCTOC.Attributes.Add("ssg_currentlastconfinementtypes", sConfinementType.TrimEnd().TrimEnd(';'));                   
                    updateCTOC.Attributes.Add("ssg_currentlastconfinementexpiry", dtExpiry);                    
                    updateCTOC.Attributes.Add("ssg_consecutivedaysinconfinement", iConfinementDays);
                    updateCTOC.Attributes.Add("ssg_separateconfinementperiodid", entity.Id);

                    _service.Update(updateCTOC);

                }
                else
                {

                }

            }
        }
    }
}
