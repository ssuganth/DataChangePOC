using System;
using System.ServiceModel;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Crm.Sdk.Messages;
using System.Linq;

namespace DH_SepCon_Workflows
{
    public class DeactivateByConfinementType : CodeActivity
    {
        IWorkflowContext _workflowContext;
        IOrganizationService _service;
        ITracingService _tracingService;

        [RequiredArgument]
        [Input("CTOC")]
        [ReferenceTarget("ssg_separateconfinementperiod")]
        public InArgument<EntityReference> CTOC { get; set; }

        [RequiredArgument]
        [Input("StartDate")]

        public InArgument<DateTime> StartDate { get; set; }

        [RequiredArgument]
        [Input("ConfinementType")]
        [AttributeTarget("ssg_separateconfinement", "ssg_separateconfinementtype_t_")]
        public InArgument<OptionSetValue> ConfinementType { get; set; }

        [RequiredArgument]
        [Input("Source Record URL")]

        public InArgument<String> SourceRecordUrl { get; set; }


        [Input("DisciplinaryFile")]
        [ReferenceTarget("ssg_violationreport")]
        public InArgument<EntityReference> DisciplinaryFile { get; set; }

        [Input("Povincial Transfer?")]
        public InArgument<Boolean> IsProvincialTransfer { get; set; }


        protected override void Execute(CodeActivityContext context)
        {
            _workflowContext = context.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = context.GetExtension<IOrganizationServiceFactory>();
            _service = serviceFactory.CreateOrganizationService(_workflowContext.InitiatingUserId);
            _tracingService = context.GetExtension<ITracingService>();
            _tracingService.Trace("Custom Workflow - DeactivateConfinmentByType : Begin");
            bool updateCTOC = false;


            String _source = this.SourceRecordUrl.Get(context);
            if (_source == null || _source == "")
            {
                return;
            }

            string[] urlParts = _source.Split("?".ToArray());
            _tracingService.Trace("Parts of Source: " + urlParts.Length);
            string[] urlParams = urlParts[1].Split("&".ToCharArray());
            string parentId = urlParams[1].Replace("id=", "");
            _tracingService.Trace("ParentId: " + parentId);

            var dtExpiry = new DateTime();
            try
            {
                EntityReference erCTOC = CTOC.Get<EntityReference>(context);
                OptionSetValue sCreatedConf = ConfinementType.Get<OptionSetValue>(context);
                //Fetch all the confinement related to CTOC
                var fetchConfinement = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
                            "<entity name='ssg_separateconfinement'>" +
                            "<attribute name='createdon' />" +
                            "<attribute name='ssg_inmateid' />" +
                            "<attribute name='ssg_number' />" +
                            "<attribute name='ssg_separateconfinementtype_t_' />" +
                            "<attribute name='ssg_separateconfinementenddate'/>" +
                            "<attribute name='ssg_violationreport'/>" +
                            "<attribute name='ownerid' />" +
                            "<attribute name='statuscode' />" +
                            "<attribute name='ssg_separateconfinementid' />" +
                            "<attribute name='ssg_date' />" +
                            "<attribute name='statecode' />" +
                            "<attribute name='ssg_actualenddatetime' />" +
                            "<attribute name='ssg_separateconfinementenddate'/>" +
                            "<attribute name='ssg_notifyingofficer'/>" +
                            "<attribute name='ssg_dateofnotification'/>" +
                            "<order attribute='ssg_inmateid' descending='false' />" +
                            "<filter type='and'>" +
                            "<condition attribute='ssg_periodofseparateconfinement' operator='eq' value='" + erCTOC.Id + "' />" +
                            "<condition attribute='statecode' value='0' operator='eq'/>" +
                            "</filter>" +
                           
                            "<link-entity name='contact' from='contactid' to='ssg_inmateid' link-type='inner' alias='aa'>" +
                             "<attribute name='ssg_correctionalcentre'/>" +     
                            "</link-entity>" +
                            "</entity>" +
                            "</fetch>";

                EntityCollection ecConfinement = _service.RetrieveMultiple(new FetchExpression(fetchConfinement));
                #region Update Confinement

                if (ecConfinement.Entities.Count > 0)
                {
                    EntityReference erBUTeam = new EntityReference();
                    EntityReference erBU = new EntityReference();

                    //If triggered by provincial transfer, then fetch the Team to which the CTOC record should be reassigned
                    if (IsProvincialTransfer.Get(context) && ecConfinement[0].Contains("aa.ssg_correctionalcentre"))
                    {
                        _tracingService.Trace("Custom Workflow - DeactivateConfinmentByType : aliasedvalue " + ecConfinement[0].GetAttributeValue<AliasedValue>("aa.ssg_correctionalcentre").Value.ToString());

                        erBU = (EntityReference)( ecConfinement[0].GetAttributeValue<AliasedValue>("aa.ssg_correctionalcentre").Value);
                        erBUTeam = FetchBUTeam((EntityReference)(ecConfinement[0].GetAttributeValue<AliasedValue>("aa.ssg_correctionalcentre").Value));
                    }
                    _tracingService.Trace("IsProvincialTransfer.Get(context): " + IsProvincialTransfer.Get(context).ToString());

                    foreach (var confinement in ecConfinement.Entities)
                    {
                        //If deactivation is triggered by s.27 or s.24
                        if (IsProvincialTransfer.Get(context)== false)
                        {
                            _tracingService.Trace("Custom Workflow - DeactivateConfinmentByType : Triggered from" + sCreatedConf.Value.ToString());
                            
                            //Deactive all the confinement other than 2.24,2.27 & MO-IND, MO-ISO
                            if (confinement.GetAttributeValue<OptionSetValue>("ssg_separateconfinementtype_t_").Value != 867670002 && confinement.GetAttributeValue<OptionSetValue>("ssg_separateconfinementtype_t_").Value != 867670003 && confinement.GetAttributeValue<OptionSetValue>("ssg_separateconfinementtype_t_").Value != 867670007 && confinement.GetAttributeValue<OptionSetValue>("ssg_separateconfinementtype_t_").Value != 867670006)
                            {
                                _tracingService.Trace("Custom Workflow - DeactivateConfinmentByType : Confinement type " + confinement.FormattedValues["ssg_separateconfinementtype_t_"].ToString());
                                UpdateConfinement(context, confinement, parentId, sCreatedConf, 2);
                                updateCTOC = true;
                            }
                            _tracingService.Trace("Custom Workflow - DeactivateConfinmentByType : Confinement Type: " + confinement.GetAttributeValue<OptionSetValue>("ssg_separateconfinementtype_t_").Value.ToString());
                            if (sCreatedConf.Value == 867670003)
                            {
                                _tracingService.Trace("Custom Workflow - DeactivateConfinmentByType : Entered s.27");
                                //fetch expirty date to update CTOC
                                if (confinement.GetAttributeValue<OptionSetValue>("ssg_separateconfinementtype_t_").Value == 867670003)
                                {
                                    _tracingService.Trace("Custom Workflow - DeactivateConfinmentByType : First Else if Confinement type " + confinement.FormattedValues["ssg_separateconfinementtype_t_"].ToString());
                                    dtExpiry = confinement.GetAttributeValue<DateTime>("ssg_separateconfinementenddate");

                                }

                                //if confinement is of type s.24, check if it is created for the same Disciplinary File.
                                else if (confinement.GetAttributeValue<OptionSetValue>("ssg_separateconfinementtype_t_").Value == 867670002 && confinement.Contains("ssg_violationreport") && confinement.GetAttributeValue<EntityReference>("ssg_violationreport").Id == DisciplinaryFile.Get<EntityReference>(context).Id)
                                {
                                    _tracingService.Trace("Custom Workflow - DeactivateConfinmentByType :Second Else if Confinement type " + confinement.FormattedValues["ssg_separateconfinementtype_t_"].ToString());
                                    //If created for the same disciplinary file, deactivate the record
                                    UpdateConfinement(context, confinement, parentId, sCreatedConf, 2);
                                    //updateCTOC = true;
                                }
                            }
                            else if (sCreatedConf.Value == 867670002)//Check the created CAR Section is S.24
                            {
                                _tracingService.Trace("Custom Workflow - DeactivateConfinmentByType : Entered s.24");
                                //fetch expirty date to update CTOC
                                if (confinement.GetAttributeValue<OptionSetValue>("ssg_separateconfinementtype_t_").Value == 867670002)
                                {
                                    _tracingService.Trace("Custom Workflow - DeactivateConfinmentByType : First Else if Confinement type " + confinement.FormattedValues["ssg_separateconfinementtype_t_"].ToString());
                                    dtExpiry = confinement.GetAttributeValue<DateTime>("ssg_separateconfinementenddate");

                                }

                            }
                        }
                        else if (IsProvincialTransfer.Get(context)) //Deactivation triggered by Provincial Transfer
                        {
                            _tracingService.Trace("Entered Provincial Transfer");
                            //Deactive all the confinement other than 2.24,2.27
                            if (confinement.GetAttributeValue<OptionSetValue>("ssg_separateconfinementtype_t_").Value != 867670002 && confinement.GetAttributeValue<OptionSetValue>("ssg_separateconfinementtype_t_").Value != 867670003)
                            {
                                _tracingService.Trace("Custom Workflow - DeactivateConfinmentByType : Confinement type " + confinement.FormattedValues["ssg_separateconfinementtype_t_"].ToString());
                                UpdateConfinement(context, confinement, parentId, sCreatedConf, 2);
                                ///updateCTOC = true;
                            }
                            else
                            {
                                if(erBU!= new EntityReference() && erBU!= null)
                                    ReassignConfinement(confinement, erBU, erBUTeam);
                            }

                        }



                    }
                }

                #endregion
                //if (updateCTOC == true)
                //{
                //    //Update CTOC attributes
                //    UpdateCTOC(context, dtExpiry, sCreatedConf, erCTOC);
                //}
                _tracingService.Trace("Custom Workflow - DeactivateConfinmentByType : End");

            }
            catch (Exception e)
            {
                throw new InvalidPluginExecutionException(e + "Custom Workflow DeactivateConfinmentByType error");
            }
        }

        /// <summary>
        /// Updates Confinement status and other related attributes
        /// </summary>
        /// <param name="context"></param>
        /// <param name="confinement"></param>
        /// <param name="parentId"></param>
        /// <param name="sCreatedConf"></param>
        public void UpdateConfinement(CodeActivityContext context, Entity confinement, String parentId, OptionSetValue sCreatedConf, int iStatus)
        {
            IWorkflowContext workflowContext = context.GetExtension<IWorkflowContext>();
            _tracingService.Trace("Custom Workflow - DeactivateConfinmentByType : Update Confinement");

            Entity updateConfinement = new Entity("ssg_separateconfinement");

           
            updateConfinement.Attributes.Add("ssg_finishextend", new OptionSetValue(867670000));
            updateConfinement.Attributes.Add("ssg_monitoringcomplete", new OptionSetValue(867670000));

            if (!confinement.Contains("ssg_notifyingofficer"))
                updateConfinement.Attributes.Add("ssg_notifyingofficer", new EntityReference("systemuser", workflowContext.InitiatingUserId));
            if (!confinement.Contains("ssg_dateofnotification"))
                updateConfinement.Attributes.Add("ssg_dateofnotification", DateTime.Now);

            if (IsProvincialTransfer.Get(context) == true)
            {
                updateConfinement.Attributes.Add("ssg_actualenddatetime", DateTime.Now);

                //Update TimeSpent in Hours - Confinement entity
                if (StartDate.Get(context) != new DateTime())
                {                    
                    var iConfinementDays = Convert.ToInt32((DateTime.Today.Date - StartDate.Get(context).Date).TotalDays + 1);
                    updateConfinement.Attributes.Add("ssg_timespentindaysvalue", iConfinementDays);
                }
            }
            else
            {
                updateConfinement.Attributes.Add("ssg_actualenddatetime", StartDate.Get<DateTime>(context));
                //Update TimeSpent in Hours - Confinement entity
                if (StartDate.Get(context) != new DateTime())
                {
                    _tracingService.Trace("StartDate.Get<DateTime>(context).Date : " + StartDate.Get<DateTime>(context).Date.ToString());
                    _tracingService.Trace("confinement.GetAttributeValue<DateTime>('ssg_date').Date : " + confinement.GetAttributeValue<DateTime>("ssg_date").Date.ToString());
                    var iConfinementDays = Convert.ToInt32((StartDate.Get<DateTime>(context).Date - confinement.GetAttributeValue<DateTime>("ssg_date").Date ).TotalDays+1);
                    updateConfinement.Attributes.Add("ssg_timespentindaysvalue", iConfinementDays);
                }
            }



            updateConfinement.Attributes.Add("statecode", new OptionSetValue(1));
            
            updateConfinement.Attributes.Add("statuscode", new OptionSetValue(iStatus));
            //updateConfinement.Attributes.Add("ssg_nextcontinuousseparateconfinement", new EntityReference("ssg_separateconfinement", new Guid(parentId)));
            updateConfinement.Attributes.Add("ssg_newendtime", StartDate.Get<DateTime>(context));
            if (confinement.Contains("ssg_separateconfinementenddate"))
                updateConfinement.Attributes.Add("ssg_originalenddatetime", confinement.GetAttributeValue<DateTime>("ssg_separateconfinementenddate"));
            
            //updateConfinement.Attributes.Add("ssg_nextperiodofconfinement", sCreatedConf);
            updateConfinement.Attributes.Add("ssg_initiationcompleted", true);
            
            updateConfinement.Attributes.Add("ssg_separateconfinementid", confinement.GetAttributeValue<Guid>("ssg_separateconfinementid"));
            updateConfinement.Attributes.Add("ssg_currentstage", "Complete");
            
            _service.Update(updateConfinement);

            //Update the BPF stage first to avoid infinte loop
            UpdateBPF(confinement);
        }

        public void ReassignConfinement( Entity confinement, EntityReference erCorrectionalCenter,EntityReference erTeam )
        {
            Entity updateConfinement = new Entity("ssg_separateconfinement");
            updateConfinement.Attributes.Add("ssg_correctionalcentre",erCorrectionalCenter);
            updateConfinement.Attributes.Add("ownerid", erTeam);
            updateConfinement.Attributes.Add("ssg_separateconfinementid", confinement.GetAttributeValue<Guid>("ssg_separateconfinementid"));
            _service.Update(updateConfinement);
        }

        /// <summary>
        /// Update CTOC //tHIS Code is commented as we are called UpdateCTOC Action after Deactivate Confinement by type
        /// </summary>
        /// <param name="context"></param>
        /// <param name="dtExpiry"></param>
        /// <param name="sCreatedConf"></param>
        /// <param name="erCTOC"></param>
        public void UpdateCTOC(ActivityContext context, DateTime dtExpiry, OptionSetValue sCreatedConf, EntityReference erCTOC)
        {
            var iConfinementDays = Convert.ToInt32((DateTime.Today - StartDate.Get<DateTime>(context).Date).TotalDays + 1);
            Entity updateCTOC = new Entity("ssg_separateconfinementperiod");

            _tracingService.Trace("Custom Workflow - DeactivateConfinmentByType : ssg_currentconfperiodstartdate " + StartDate.Get<DateTime>(context).ToString());
            _tracingService.Trace("Custom Workflow - DeactivateConfinmentByType : ssg_currentlastconfinementexpiry " + dtExpiry.ToString());

            //if (StartDate.Get<DateTime>(context).ToShortDateString() != "01/01/0001")
            //updateCTOC.Attributes.Add("ssg_currentconfperiodstartdate", StartDate.Get<DateTime>(context));
            if (dtExpiry.ToShortDateString() != "01/01/0001")
                updateCTOC.Attributes.Add("ssg_currentlastconfinementexpiry", dtExpiry);

            updateCTOC.Attributes.Add("ssg_currentlastconfinementtypes", sCreatedConf + ";");
            updateCTOC.Attributes.Add("ssg_separateconfinementperiodid", erCTOC.Id);
            updateCTOC.Attributes.Add("ssg_consecutivedaysinconfinement", iConfinementDays);
            _tracingService.Trace("Custom Workflow - DeactivateConfinmentByType : Update CTOC " + erCTOC.Id.ToString());
            _service.Update(updateCTOC);
        }

        public void UpdateBPF(Entity confinement)
        {
            if (confinement.GetAttributeValue<OptionSetValue>("ssg_separateconfinementtype_t_").Value != 867670003 && confinement.GetAttributeValue<OptionSetValue>("ssg_separateconfinementtype_t_").Value != 867670006 && confinement.GetAttributeValue<OptionSetValue>("ssg_separateconfinementtype_t_").Value != 867670007)
            {
                //Retrieve BPF
                RetrieveProcessInstancesRequest processInstanceRequest = new RetrieveProcessInstancesRequest { EntityId = confinement.Id, EntityLogicalName = "ssg_separateconfinement" };
                RetrieveProcessInstancesResponse processInstanceResponse = (RetrieveProcessInstancesResponse)_service.Execute(processInstanceRequest);

                int processCount = processInstanceResponse.Processes.Entities.Count;
                Entity activeProcessInstance = processInstanceResponse.Processes.Entities[0];
                Guid activeProcessInstanceID = activeProcessInstance.Id;
                _tracingService.Trace("Custom Workflow - DeactivateConfinmentByType : Active BPF: " + activeProcessInstance.LogicalName);

                var _activeStageId = new Guid(activeProcessInstance.Attributes["processstageid"].ToString());

                //Fetch active path of the bpf to find the active stage name.
                RetrieveActivePathRequest activePathRequest = new RetrieveActivePathRequest { ProcessInstanceId = activeProcessInstance.Id };
                RetrieveActivePathResponse pathResp = (RetrieveActivePathResponse)_service.Execute(activePathRequest);
                var activeStageName = "";
                var _activeStagePosition = 0;

                for (int i = 0; i < pathResp.ProcessStages.Entities.Count; i++)
                {
                    _tracingService.Trace("\tStage {0}: {1} (StageId: {2})", i + 1,
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
                    _tracingService.Trace("Custom Workflow - DeactivateConfinmentByType : _activeStagePosition In BPF: " + _activeStagePosition.ToString());
                    _tracingService.Trace("Custom Workflow - DeactivateConfinmentByType : pathResp.ProcessStages.Entities.Count In BPF: " + pathResp.ProcessStages.Entities.Count.ToString());
                    var newActiveStage = _activeStagePosition + 1;
                    _tracingService.Trace("Custom Workflow - DeactivateConfinmentByType : New Process STage In BPF: " + newActiveStage.ToString());
                    _activeStageId = (Guid)pathResp.ProcessStages.Entities[newActiveStage].Attributes["processstageid"];
                    _activeStagePosition = _activeStagePosition + 1;


                    //Retrieve the process instance record to update its active stage 
                    ColumnSet cols1 = new ColumnSet(); cols1.AddColumn("activestageid");
                    Entity retrievedProcessInstance = _service.Retrieve("new_bpf_ff66fb64588d4e5682707114d9510844", activeProcessInstanceID, cols1);
                    // Set the next stage as the active stage 
                    retrievedProcessInstance["activestageid"] = new EntityReference("processstage", _activeStageId);
                    _service.Update(retrievedProcessInstance);
                }


                _tracingService.Trace("Custom Workflow - DeactivateConfinmentByType : Confinement Type In BPF: " + confinement.GetAttributeValue<OptionSetValue>("ssg_separateconfinementtype_t_").Value.ToString());
                //var stateRequest = new SetStateRequest
                //{
                //    EntityMoniker = new EntityReference("new_bpf_ff66fb64588d4e5682707114d9510844", activeProcessInstance.Id),
                //    State = new OptionSetValue(1), // Inactive.
                //    Status = new OptionSetValue(2) // Finished.
                //};
                //_service.Execute(stateRequest);
            }

        }

        public EntityReference FetchBUTeam(EntityReference erBU)
        {
            try
            {                
                var sBUName = erBU.Name;

                if (sBUName == "CB-ICAP-STAGE") //Including this to accomadate change in new UAT environemnt
                    sBUName = "CB-ICAP";

                var fetchTeam = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
                                    "<entity name='team'>" +
                                    "<attribute name='name'/>" +
                                    "<attribute name='businessunitid'/>" +
                                    "<attribute name='teamid'/>" +
                                    "<attribute name='teamtype'/>" +
                                    "<order attribute='name' descending='false'/>" +
                                    "<filter type='and'>" +
                                    "<condition attribute='name' operator='eq' value='" + sBUName + "'/>" +
                                    "</filter>" +
                                    "</entity>" +
                                    "</fetch>";

                EntityCollection ecTeams = _service.RetrieveMultiple(new FetchExpression(fetchTeam));

                if (ecTeams.Entities.Count == 1)
                {
                   return new EntityReference("team", ecTeams.Entities[0].Id);

                }
                else
                    return null;
            }

            catch (Exception e)
            {
                throw new InvalidPluginExecutionException(e + "Custom Workflow FinishHBPF error");
            }
        }
    }
}
