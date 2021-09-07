using System;
using System.ServiceModel;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Crm.Sdk.Messages;

namespace DH_SepCon_Workflows
{
    public class FetchBUsTeam : CodeActivity
    {
        IWorkflowContext _workflowContext;
        IOrganizationService _service;
        ITracingService _tracingService;

        [Input("BusinessUnit")]
        [ReferenceTarget("businessunit")]
        public InArgument<EntityReference> BusinessUnit { get; set; }

        [Output("Teams")]
        [ReferenceTarget("team")]
        public OutArgument<EntityReference> teams { get; set; }
        
        protected override void Execute(CodeActivityContext context)
        {
            _workflowContext = context.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = context.GetExtension<IOrganizationServiceFactory>();
            _service = serviceFactory.CreateOrganizationService(_workflowContext.InitiatingUserId);
            _tracingService = context.GetExtension<ITracingService>();
            _tracingService.Trace("Custom Workflow - Finish BPF : Begin");

            try
            {
                EntityReference erBU = BusinessUnit.Get<EntityReference>(context);
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
                                    "<condition attribute='name' operator='eq' value='"+sBUName+"'/>" +
                                    "</filter>" +
                                    "</entity>" +
                                    "</fetch>";

                EntityCollection ecTeams = _service.RetrieveMultiple(new FetchExpression(fetchTeam));

                if (ecTeams.Entities.Count == 1)
                {
                    this.teams.Set(context, new EntityReference("team", ecTeams.Entities[0].Id));

                }
            }

            catch (Exception e)
            {
                throw new InvalidPluginExecutionException(e + "Custom Workflow FinishHBPF error");
            }
        }
    }
}
