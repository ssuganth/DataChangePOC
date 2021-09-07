using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DH_SepCon_Plugins
{
    public class OnUpdateConfinement : IPlugin
    {
        IOrganizationService _service;
        IPluginExecutionContext _context;
        ITracingService trace;
        public void Execute(IServiceProvider serviceProvider)
        {
            _context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
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
                trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));


                var statecode = entity.GetAttributeValue<OptionSetValue>("statecode");
                var statuscode = entity.GetAttributeValue<OptionSetValue>("statuscode");
                var actualEnd = entity.GetAttributeValue<DateTime>("ssg_actualenddatetime");
                var startDate = entity.Contains("ssg_date") ? entity.GetAttributeValue<DateTime>("ssg_date") : new DateTime();
                Guid client;
                //code should be triggered only on status update
                if (statecode != null && statuscode != null)
                {
                    if (statecode.Value == 1 && statuscode.Value == 2) //Ended
                    {
                        trace.Trace("Entered Ended status loop");
                        EntityReference erCTOC = null;
                        String currentConfinementType = String.Empty;
                        int currentConfinementTypeValue = 0;

                        if (_context.PreEntityImages.Contains("PreImage") && _context.PreEntityImages["PreImage"] is Entity)
                        {
                            Entity preMessageImage = (Entity)_context.PreEntityImages["PreImage"];
                            erCTOC = preMessageImage.GetAttributeValue<EntityReference>("ssg_periodofseparateconfinement");
                            if (actualEnd.ToShortDateString() == "1/1/0001")
                                actualEnd = preMessageImage.GetAttributeValue<DateTime>("ssg_actualenddatetime");
                            if (startDate == new DateTime())
                                startDate = preMessageImage.GetAttributeValue<DateTime>("ssg_date");
                            currentConfinementType = preMessageImage.FormattedValues["ssg_separateconfinementtype_t_"].ToString();
                            currentConfinementTypeValue = preMessageImage.GetAttributeValue<OptionSetValue>("ssg_separateconfinementtype_t_").Value;
                            client = preMessageImage.GetAttributeValue<EntityReference>("ssg_inmateid").Id;
                        }

                        //UpdateBPF(currentConfinementTypeValue,entity);
                        //continue with the functionality only if there is a CTOC record associated with Confinement
                        if (erCTOC != null)
                        {
                            try
                            {
                                
                                //Fetch all the Confinement record related to CTOC
                                var fetchConfinement = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
                                                "<entity name='ssg_separateconfinement'>" +
                                                "<attribute name='createdon' />" +
                                                "<attribute name='ssg_inmateid' />" +
                                                "<attribute name='ssg_number' />" +
                                                "<attribute name='ssg_separateconfinementtype_t_' />" +
                                                "<attribute name='ownerid' />" +
                                                "<attribute name='statuscode' />" +
                                                "<attribute name='ssg_separateconfinementid' />" +
                                                "<attribute name='statecode' />" +
                                                "<attribute name='ssg_actualenddatetime' />" +
                                                "<order attribute='ssg_inmateid' descending='false' />" +
                                                "<filter type='and'>" +
                                                "<condition attribute='ssg_periodofseparateconfinement' operator='eq' value='"+erCTOC.Id+"' />" +
                                                "<condition attribute='statecode' value='0' operator='eq'/>" +
                                                "</filter>" +
                                                "</entity>" +
                                                "</fetch>";

                                EntityCollection ecConfinement = _service.RetrieveMultiple(new FetchExpression(fetchConfinement));
                                trace.Trace("Count of Confinements: "+ ecConfinement.Entities.Count().ToString());
                                //if there are active confinements, update CTOC accordingly
                                if (ecConfinement.Entities.Count() > 0)
                                {
                                    //confinement type in CTOC is a comma separated value of all the active confinement types
                                    var sConfinementType = String.Empty;
                                    Entity updateCTOC = new Entity("ssg_separateconfinementperiod");

                                    foreach (var confinement in ecConfinement.Entities)
                                    {
                                        sConfinementType += confinement.FormattedValues["ssg_separateconfinementtype_t_"].ToString();
                                        sConfinementType += "; ";
                                        trace.Trace("Before check Confinement type: " + confinement.GetAttributeValue<OptionSetValue>("ssg_separateconfinementtype_t_").Value.ToString());
                                        
                                    }
                                    trace.Trace("Current Confinement type: " + currentConfinementTypeValue.ToString());
                                    if (currentConfinementTypeValue == 867670004 || currentConfinementTypeValue == 867670000)
                                    {
                                        trace.Trace("Current Confinement type: " + currentConfinementTypeValue.ToString());
                                        updateCTOC.Attributes.Add("ssg_s17expirydatetime", null);
                                    }

                                    updateCTOC.Attributes.Add("ssg_currentlastconfinementtypes", sConfinementType.TrimEnd().TrimEnd(';'));
                                    updateCTOC.Attributes.Add("ssg_separateconfinementperiodid", erCTOC.Id);

                                    //_service.Update(updateCTOC);

                                }
                                //if there are no active confinements, update the actual end date in CTOC and deactivate the same
                                else
                                {
                                    Entity updateCTOC = new Entity("ssg_separateconfinementperiod");


                                    var fetchCTOC = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
                                                    "<entity name='ssg_separateconfinementperiod'>" +
                                                    "<attribute name='ssg_separateconfinementperiodid' />" +
                                                    "<attribute name='ssg_name' />" +
                                                    "<attribute name='ssg_client'/>" +
                                                    "<attribute name='ssg_confinementstartdate' />" +
                                                    "<order attribute='ssg_name' descending='false' />" +
                                                    "<filter type='and'>" +
                                                    "<condition attribute='ssg_separateconfinementperiodid' operator='eq' value='" + erCTOC.Id + "'/>  " +
                                                    "</filter>" +
                                                    "</entity>" +
                                                    "</fetch>";

                                    EntityCollection ecCTOC = _service.RetrieveMultiple(new FetchExpression(fetchCTOC));
                                    var confinementDays = 0;
                                    
                                    if (ecCTOC.Entities.Count() > 0)
                                    {
                                        var confinementStart = ecCTOC.Entities[0].GetAttributeValue <DateTime>("ssg_confinementstartdate");

                                        confinementDays = Convert.ToInt32((actualEnd.Date - confinementStart.Date).TotalDays);

                                        trace.Trace("ActualEnd: " + actualEnd.ToString());
                                        trace.Trace("Confinement Start: " + confinementStart.Date);
                                        trace.Trace("Total Days: " + (actualEnd.Date - confinementStart.Date).TotalDays.ToString());
                                    }



                                    updateCTOC.Attributes.Add("ssg_s17expirydatetime", null);
                                    updateCTOC.Attributes.Add("ssg_actualconfinementend", actualEnd);
                                    updateCTOC.Attributes.Add("ssg_currentlastconfinementexpiry", actualEnd);
                                    updateCTOC.Attributes.Add("ssg_currentlastconfinementtypes", currentConfinementType);
                                    updateCTOC.Attributes.Add("ssg_consecutivedaysinconfinement", confinementDays);
                                    updateCTOC.Attributes.Add("statecode", new OptionSetValue(1));
                                    updateCTOC.Attributes.Add("statuscode", new OptionSetValue(2));
                                    updateCTOC.Attributes.Add("ssg_separateconfinementperiodid", erCTOC.Id);
                                    

                                    _service.Update(updateCTOC);

                                    
                                    Entity updateClient = new Entity("contact");
                                    updateClient.Attributes.Add("contactid", ecCTOC.Entities[0].GetAttributeValue<EntityReference>("ssg_client").Id);
                                    updateClient.Attributes.Add("ssg_clientoutofconfinement", new OptionSetValue(867670000)); //true
                                    _service.Update(updateClient);

                                }
                            }
                            catch (Exception e)
                            {
                                throw new InvalidPluginExecutionException(e + "PostCreateInmateAssessment Plugin error");
                            }
                        }

                       
                    }
                    else if(statecode.Value==1 && statuscode.Value== 867670000) //cancel
                    {
                        trace.Trace("Confinement is cancelled");
                        EntityReference erCTOC = null;
                        String currentConfinementType = String.Empty;

                        if (_context.PreEntityImages.Contains("PreImage") && _context.PreEntityImages["PreImage"] is Entity)
                        {
                            Entity preMessageImage = (Entity)_context.PreEntityImages["PreImage"];
                            erCTOC = preMessageImage.GetAttributeValue<EntityReference>("ssg_periodofseparateconfinement");
                            if (actualEnd.ToShortDateString() == "1/1/0001")
                                actualEnd = preMessageImage.GetAttributeValue<DateTime>("ssg_actualenddatetime");
                            currentConfinementType = preMessageImage.FormattedValues["ssg_separateconfinementtype_t_"].ToString();
                        }

                        if (erCTOC != null)
                        {
                            try
                            {

                                //Fetch all confinements for the related CTOC and active
                                var fetchConfinement = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
                                                "<entity name='ssg_separateconfinement'>" +
                                                "<attribute name='createdon' />" +
                                                "<attribute name='ssg_inmateid' />" +
                                                "<attribute name='ssg_number' />" +
                                                "<attribute name='ssg_separateconfinementtype_t_' />" +
                                                "<attribute name='ownerid' />" +
                                                "<attribute name='statuscode' />" +
                                                "<attribute name='ssg_separateconfinementid' />" +
                                                "<attribute name='statecode' />" +
                                                "<attribute name='ssg_actualenddatetime' />" +
                                                "<order attribute='ssg_separateconfinementenddate' descending='true' />" +
                                                "<filter type='and'>" +
                                                "<condition attribute='ssg_periodofseparateconfinement' operator='eq' value='" + erCTOC.Id + "' />" +
                                                "<condition attribute='statecode' value='0' operator='eq'/>" +
                                                "</filter>" +
                                                "</entity>" +
                                                "</fetch>";

                                EntityCollection ecConfinement = _service.RetrieveMultiple(new FetchExpression(fetchConfinement));
                                if (ecConfinement.Entities.Count() > 0)
                                {
                                    
                                    var dtCARStart = new DateTime();
                                    var dtMOStartDate = new DateTime();
                                   
                                    var iConfinementDays = 0;
                                    var sMOConfinementType = String.Empty;
                                    var sConfinementType = String.Empty;
                                    foreach (var confinement in ecConfinement.Entities)
                                    {
                                        sConfinementType += confinement.FormattedValues["ssg_separateconfinementtype_t_"].ToString();
                                        sConfinementType += "; ";

                                        if (confinement.GetAttributeValue<OptionSetValue>("ssg_separateconfinementtype_t_").Value != 867670006 && confinement.GetAttributeValue<OptionSetValue>("ssg_separateconfinementtype_t_").Value != 867670007)
                                        {
                                            trace.Trace("Custom Workflow - UpdateCTOC : Confinement CAR Type");
                                            if (dtCARStart != new DateTime() && dtCARStart > confinement.GetAttributeValue<DateTime>("ssg_date"))
                                                dtCARStart = confinement.GetAttributeValue<DateTime>("ssg_date");
                                            else if (dtCARStart == new DateTime())
                                                dtCARStart = confinement.GetAttributeValue<DateTime>("ssg_date");

                                        }
                                        else // If Confinement is of MO Type
                                        {
                                            trace.Trace("Custom Workflow - UpdateCTOC : Confinement MO Type");
                                            if (dtMOStartDate != new DateTime() && dtMOStartDate > confinement.GetAttributeValue<DateTime>("ssg_date"))
                                                dtMOStartDate = confinement.GetAttributeValue<DateTime>("ssg_date");
                                            else if (dtMOStartDate == new DateTime())
                                                dtMOStartDate = confinement.GetAttributeValue<DateTime>("ssg_date");
                                            if (confinement.GetAttributeValue<OptionSetValue>("ssg_separateconfinementtype_t_").Value == 867670006)
                                                sMOConfinementType = sMOConfinementType + "MO-IND;";
                                            else
                                                sMOConfinementType = sMOConfinementType + "MO-ISO;";
                                        }
                                    }

                                    Entity updateCTOC = new Entity("ssg_separateconfinementperiod");

                                    updateCTOC.Attributes.Add("ssg_currentlastconfinementtypes", sConfinementType.TrimEnd().TrimEnd(';'));
                                    updateCTOC.Attributes.Add("ssg_separateconfinementperiodid", erCTOC.Id);

                                    updateCTOC.Attributes.Add("ssg_covid", sMOConfinementType.TrimEnd().TrimEnd(';'));
                                    if (dtCARStart != new DateTime() )
                                    {
                                        iConfinementDays = Convert.ToInt32((DateTime.Today - dtCARStart.Date).TotalDays + 1);
                                        updateCTOC.Attributes.Add("ssg_currentconfinementperiodstartdate", dtCARStart);
                                    }
                                    else if (dtCARStart == new DateTime() && dtMOStartDate != new DateTime())
                                    {
                                        iConfinementDays = Convert.ToInt32((DateTime.Today - dtMOStartDate.Date).TotalDays + 1);
                                        updateCTOC.Attributes.Add("ssg_currentconfinementperiodstartdate", dtMOStartDate);
                                    }

                                    _service.Update(updateCTOC);

                                }
                                else
                                {
                                    trace.Trace("No Active Confinements for the related CTOC");
                                    //Fetch all confinement related to CTOC and Inactive
                                    var fetchInactiveConfinement = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
                                                    "<entity name='ssg_separateconfinement'>" +
                                                    "<attribute name='ssg_separateconfinementenddate' />" +
                                                    "<attribute name='ssg_inmateid' />" +
                                                    "<attribute name='ssg_number' />" +
                                                    "<attribute name='ssg_separateconfinementtype_t_' />" +
                                                    "<attribute name='ownerid' />" +
                                                    "<attribute name='statuscode' />" +
                                                    "<attribute name='ssg_separateconfinementid' />" +
                                                    "<attribute name='statecode' />" +
                                                    "<attribute name='ssg_actualenddatetime' />" +
                                                    "<order attribute='ssg_actualenddatetime' descending='true' />" +
                                                    "<filter type='and'>" +
                                                    "<condition attribute='ssg_periodofseparateconfinement' operator='eq' value='" + erCTOC.Id + "' />" +
                                                    "<condition attribute='statecode' value='1' operator='eq'/>" +
                                                    "</filter>" +
                                                    "</entity>" +
                                                    "</fetch>";

                                    EntityCollection ecConf = _service.RetrieveMultiple(new FetchExpression(fetchInactiveConfinement));

                                    if (ecConf.Entities.Count() > 0)
                                    {
                                        
                                        actualEnd = ecConf.Entities[0].GetAttributeValue<DateTime>("ssg_actualenddatetime");
                                        currentConfinementType = ecConf.Entities[0].FormattedValues["ssg_separateconfinementtype_t_"].ToString();
                                        var expiryDate = ecConf.Entities[0].GetAttributeValue<DateTime>("ssg_separateconfinementenddate");

                                        trace.Trace("Last Active Confinement Type: " + currentConfinementType);


                                        Entity updateCTOC = new Entity("ssg_separateconfinementperiod");


                                        var fetchCTOC = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
                                                        "<entity name='ssg_separateconfinementperiod'>" +
                                                        "<attribute name='ssg_separateconfinementperiodid' />" +
                                                        "<attribute name='ssg_name' />" +
                                                        "<attribute name='ssg_confinementstartdate' />" +
                                                        "<order attribute='ssg_name' descending='false' />" +
                                                        "<filter type='and'>" +
                                                        "<condition attribute='ssg_separateconfinementperiodid' operator='eq' value='" + erCTOC.Id + "'/>  " +
                                                        "</filter>" +
                                                        "</entity>" +
                                                        "</fetch>";

                                        EntityCollection ecCTOC = _service.RetrieveMultiple(new FetchExpression(fetchCTOC));
                                        var confinementDays = 0;
                                        if (ecCTOC.Entities.Count() > 0)
                                        {
                                            var confinementStart = ecCTOC.Entities[0].GetAttributeValue<DateTime>("ssg_confinementstartdate");

                                            //confinementDays = Convert.ToInt32((actualEnd.Date - confinementStart.Date).TotalDays) ;

                                            confinementDays = Convert.ToInt32((actualEnd.Date - confinementStart.Date).TotalDays + 1);

                                            trace.Trace("ActualEnd: " + actualEnd.ToString());
                                            trace.Trace("Confinement Start: " + confinementStart.Date);
                                            if (actualEnd.ToShortDateString() != "1/1/0001")
                                                trace.Trace("Total Days: " + (actualEnd.Date - confinementStart.Date).TotalDays.ToString());
                                        }


                                        if(actualEnd.ToShortDateString() != "1/1/0001" )
                                            updateCTOC.Attributes.Add("ssg_actualconfinementend", actualEnd);
                                        updateCTOC.Attributes.Add("ssg_currentlastconfinementexpiry", expiryDate);
                                        updateCTOC.Attributes.Add("ssg_currentlastconfinementtypes", currentConfinementType.TrimEnd().TrimEnd(';'));
                                        updateCTOC.Attributes.Add("ssg_consecutivedaysinconfinement", confinementDays);
                                        updateCTOC.Attributes.Add("statecode", new OptionSetValue(1));
                                        updateCTOC.Attributes.Add("statuscode", new OptionSetValue(2));
                                        updateCTOC.Attributes.Add("ssg_separateconfinementperiodid", erCTOC.Id);
                                        updateCTOC.Attributes.Add("ssg_currentconfinementperiodstartdate", null);
                                        updateCTOC.Attributes.Add("ssg_covid", String.Empty);
                                        _service.Update(updateCTOC);
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
        }


        public void UpdateBPF(int confinementTypeValue, Entity confinement)
        {
            if (confinementTypeValue != 867670003 && confinementTypeValue != 867670005)
            {
                //Retrieve BPF
                RetrieveProcessInstancesRequest processInstanceRequest = new RetrieveProcessInstancesRequest { EntityId = confinement.Id, EntityLogicalName = "ssg_separateconfinement" };
                RetrieveProcessInstancesResponse processInstanceResponse = (RetrieveProcessInstancesResponse)_service.Execute(processInstanceRequest);

                int processCount = processInstanceResponse.Processes.Entities.Count;
                Entity activeProcessInstance = processInstanceResponse.Processes.Entities[0];
                Guid activeProcessInstanceID = activeProcessInstance.Id;
                trace.Trace("Plugin - OnUpdateConfinement : Active BPF: " + activeProcessInstance.LogicalName);

                var _activeStageId = new Guid(activeProcessInstance.Attributes["processstageid"].ToString());

                //Fetch active path of the bpf to find the active stage name.
                RetrieveActivePathRequest activePathRequest = new RetrieveActivePathRequest { ProcessInstanceId = activeProcessInstance.Id };
                RetrieveActivePathResponse pathResp = (RetrieveActivePathResponse)_service.Execute(activePathRequest);
                var activeStageName = "";
                var _activeStagePosition = 0;

                for (int i = 0; i < pathResp.ProcessStages.Entities.Count; i++)
                {
                    trace.Trace("\tStage {0}: {1} (StageId: {2})", i + 1,
                                            pathResp.ProcessStages.Entities[i].Attributes["stagename"],
                                            pathResp.ProcessStages.Entities[i].Attributes["processstageid"]);

                    // Retrieve the active stage name and active stage position based on the activeStageId for the process instance
                    if (pathResp.ProcessStages.Entities[i].Attributes["processstageid"].ToString() == _activeStageId.ToString())
                    {
                        activeStageName = pathResp.ProcessStages.Entities[i].Attributes["stagename"].ToString();
                        _activeStagePosition = i;
                    }
                }

                //Loop the stages until it reaches Complete --> Last stage is Cancel, so we are mentioning as count-2
                while (_activeStagePosition < pathResp.ProcessStages.Entities.Count - 2)
                {
                    trace.Trace("Plugin - OnUpdateConfinement : _activeStagePosition In BPF: " + _activeStagePosition.ToString());
                    trace.Trace("Plugin - OnUpdateConfinement : pathResp.ProcessStages.Entities.Count In BPF: " + pathResp.ProcessStages.Entities.Count.ToString());
                    var newActiveStage = _activeStagePosition + 1;
                    trace.Trace("Plugin - OnUpdateConfinement : New Process STage In BPF: " + newActiveStage.ToString());
                    _activeStageId = (Guid)pathResp.ProcessStages.Entities[newActiveStage].Attributes["processstageid"];
                    _activeStagePosition = _activeStagePosition + 1;


                    //Retrieve the process instance record to update its active stage 
                    ColumnSet cols1 = new ColumnSet(); cols1.AddColumn("activestageid");
                    Entity retrievedProcessInstance = _service.Retrieve("new_bpf_ff66fb64588d4e5682707114d9510844", activeProcessInstanceID, cols1);
                    // Set the next stage as the active stage 
                    retrievedProcessInstance["activestageid"] = new EntityReference("processstage", _activeStageId);
                    _service.Update(retrievedProcessInstance);
                }


                trace.Trace("Plugin - OnUpdateConfinement : Confinement Type In BPF: " + confinementTypeValue.ToString());
                var stateRequest = new SetStateRequest
                {
                    EntityMoniker = new EntityReference("new_bpf_ff66fb64588d4e5682707114d9510844", activeProcessInstance.Id),
                    State = new OptionSetValue(1), // Inactive.
                    Status = new OptionSetValue(2) // Finished.
                };
                _service.Execute(stateRequest);
            }

        }
    }
}
