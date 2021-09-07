using System;
using System.ServiceModel;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Crm.Sdk.Messages;

namespace DH_SepCon_Workflows
{
    public class Finish_DH_BPF : CodeActivity
    {
        IWorkflowContext _workflowContext;
        IOrganizationService _service;
        ITracingService _tracingService;

        [Input("DisciplinaryHearing")]
        [ReferenceTarget("ssg_disciplinaryhearing")]
        public InArgument<EntityReference> DisciplinaryHearing { get; set; }
        protected override void Execute(CodeActivityContext context)
        {
            _workflowContext = context.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = context.GetExtension<IOrganizationServiceFactory>();
            _service = serviceFactory.CreateOrganizationService(_workflowContext.InitiatingUserId);
            _tracingService = context.GetExtension<ITracingService>();
            _tracingService.Trace("Custom Workflow - Finish BPF : Begin");

            try
            {
                EntityReference erDH = DisciplinaryHearing.Get<EntityReference>(context);
                var guidDHId = erDH.Id;

                //Retrieve BPF
                RetrieveProcessInstancesRequest processInstanceRequest = new RetrieveProcessInstancesRequest { EntityId = guidDHId, EntityLogicalName = "ssg_disciplinaryhearing" };
                RetrieveProcessInstancesResponse processInstanceResponse = (RetrieveProcessInstancesResponse)_service.Execute(processInstanceRequest);
                
                int processCount = processInstanceResponse.Processes.Entities.Count;
                Entity activeProcessInstance = processInstanceResponse.Processes.Entities[0];                
                Guid activeProcessInstanceID = activeProcessInstance.Id;

                //Find the active Stage
                var activeStageID = activeProcessInstance.Attributes["processstageid"];

                //Fetch active path of the bpf to find the active stage name.
                RetrieveActivePathRequest activePathRequest = new RetrieveActivePathRequest { ProcessInstanceId = activeProcessInstance.Id };
                RetrieveActivePathResponse pathResp = (RetrieveActivePathResponse)_service.Execute(activePathRequest);
                var activeStageName = "";
                for (int i = 0; i < pathResp.ProcessStages.Entities.Count; i++)
                {
                    // Retrieve the active stage name and active stage position based on the activeStageId for the process instance
                    if (pathResp.ProcessStages.Entities[i].Attributes["processstageid"].ToString() == activeStageID.ToString())
                    {
                        activeStageName = pathResp.ProcessStages.Entities[i].Attributes["stagename"].ToString();
                        break;

                    }
                }

                //If active stage is Pending Appeal, then finish the bpf
                if (activeStageName == "Pending Appeal")
                {
                    var stateRequest = new SetStateRequest
                    {
                        EntityMoniker = new EntityReference("ssg_disciplinaryhearingsbpf", activeProcessInstance.Id),
                        State = new OptionSetValue(1), // Inactive.
                        Status = new OptionSetValue(2) // Finished.
                    };
                    _service.Execute(stateRequest);
                }
            }
            catch (Exception e)
            {
                throw new InvalidPluginExecutionException(e + "Custom Workflow FinishHBPF error");
            }
        }
    }
}
