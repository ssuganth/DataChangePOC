using System;
using System.ServiceModel;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Crm.Sdk.Messages;

namespace DH_SepCon_Workflows
{
    public class CheckCTOCStatus:CodeActivity
    {
        IWorkflowContext _workflowContext;
        IOrganizationService _service;
        ITracingService _tracingService;

        [RequiredArgument]
        [Input("CTOC")]
        [ReferenceTarget("ssg_separateconfinementperiod")]
        public InArgument<EntityReference> CTOC { get; set; }

        [RequiredArgument]
        [Input("TermID")]
        public InArgument<String> TermId { get; set; }

        [Output("Status")]
        public OutArgument<String> CTOCStatus { get; set; }

        //Check CTOC's status and return the value
        protected override void Execute(CodeActivityContext context)
        {
            _workflowContext = context.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = context.GetExtension<IOrganizationServiceFactory>();
            _service = serviceFactory.CreateOrganizationService(_workflowContext.InitiatingUserId);
            _tracingService = context.GetExtension<ITracingService>();
            _tracingService.Trace("Custom Workflow - CheckRecordStatus : Begin");

            try
            {
                EntityReference erCTOC = CTOC.Get<EntityReference>(context);

                _tracingService.Trace("CTOC ID: " + erCTOC.Id);
                var erRetrievedCTOC = _service.Retrieve("ssg_separateconfinementperiod", erCTOC.Id, new ColumnSet("ssg_separateconfinementperiodid", "statecode", "ssg_termid"));
                _tracingService.Trace("CTOC status: " + erRetrievedCTOC.GetAttributeValue<OptionSetValue>("statecode").Value.ToString());
                _tracingService.Trace("CTOC Term ID: " + TermId.Get<String>(context));

                String sCTOCTerm = String.Empty, sClientTerm = String.Empty;
                sCTOCTerm = erRetrievedCTOC.Contains("ssg_termid") ? erRetrievedCTOC.GetAttributeValue<String>("ssg_termid") : String.Empty;
                _tracingService.Trace("sCTOCTerm " + sCTOCTerm);
                sClientTerm = TermId.Get<String>(context);
                if (erRetrievedCTOC.GetAttributeValue<OptionSetValue>("statecode").Value == 0 && sClientTerm!=String.Empty && sCTOCTerm == TermId.Get<String>(context))
                {
                    this.CTOCStatus.Set(context, "Active");
                }
                else if (erRetrievedCTOC.GetAttributeValue<OptionSetValue>("statecode").Value == 1 && sClientTerm != String.Empty && sCTOCTerm == TermId.Get<String>(context))
                {
                    _tracingService.Trace("In Term And Inactive");
                    this.CTOCStatus.Set(context, "In Term And Inactive");
                }
                else
                {
                    _tracingService.Trace("Inactive");
                    this.CTOCStatus.Set(context, "Inactive");
                }


            }
            catch (Exception e)
            {
                throw new InvalidPluginExecutionException(e + "Custom Workflow CheckRecordStatus error");
            }
        }
    }
}
