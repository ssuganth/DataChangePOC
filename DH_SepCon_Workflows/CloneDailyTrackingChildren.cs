using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Crm.Sdk.Messages;

namespace DH_SepCon_Workflows
{
    public class CloneDailyTrackingChildren: CodeActivity
    {
        IWorkflowContext _workflowContext;
        IOrganizationService _service;
        ITracingService _tracingService;

        #region "Parameter Definition"

        [RequiredArgument]
        [Input("Source Record URL")]
        [ReferenceTarget("")]
        public InArgument<String> SourceRecordUrl { get; set; }

        [RequiredArgument]
        [Input("Target Record URL")]
        [ReferenceTarget("")]
        public InArgument<String> TargetRecordUrl { get; set; }       

        

        #endregion

        protected override void Execute(CodeActivityContext context)
        {
            _workflowContext = context.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = context.GetExtension<IOrganizationServiceFactory>();
            _service = serviceFactory.CreateOrganizationService(_workflowContext.InitiatingUserId);
            _tracingService = context.GetExtension<ITracingService>();
            _tracingService.Trace("Custom Workflow - Finish BPF : Begin");


            #region "Read Parameters"

            
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

            String _destination = this.TargetRecordUrl.Get(context);
            if (_destination == null || _destination == "")
            {
                return;
            }
            string[] destinationUrlParts = _destination.Split("?".ToArray());
            string[] destinationUrlParams = destinationUrlParts[1].Split("&".ToCharArray());
            string destinationId = destinationUrlParams[1].Replace("id=", "");
            _tracingService.Trace("DestinationId: " + destinationId);

            #endregion

            try
            {
                GenerateLeaves(parentId, destinationId);
                GenerateClientLogsManual(parentId, destinationId);
                GenerateClientLogs(parentId, destinationId);
            }
            catch (Exception e)
            {
                throw new InvalidPluginExecutionException(e + "Custom Workflow FinishHBPF error");
            }
        }

        public void GenerateLeaves(String parentId, String destinationId)
        {
            ConditionExpression condition1 = new ConditionExpression();
            condition1.AttributeName = "ssg_dailytracking";
            condition1.Operator = ConditionOperator.Equal;
            condition1.Values.Add(parentId);

            FilterExpression filter1 = new FilterExpression();
            filter1.Conditions.Add(condition1);

            QueryExpression query = new QueryExpression("ssg_leavevisit");
            query.ColumnSet.AddColumns("ssg_leavevisitid");
            query.Criteria.AddFilter(filter1);

            EntityCollection ecLeaves = _service.RetrieveMultiple(query);
            int iLeaveSequence = 0;

            _tracingService.Trace("Count of Leaves: " + ecLeaves.Entities.Count.ToString());
            if (ecLeaves.Entities.Count > 0)
            {
                _tracingService.Trace("Count of Leaves: " + ecLeaves.Entities.Count.ToString());
                foreach (var leaves in ecLeaves.Entities)
                {
                    ++iLeaveSequence;
                    Entity leave = new Entity("ssg_leavevisit");
                    leave.Attributes.Add("ssg_leavevisitid", leaves.Id);
                    leave.Attributes.Add("ssg_dailytracking", new EntityReference("ssg_dailytracking", new Guid(destinationId)));
                    _service.Update(leave);
                }

                
                
            }
            Entity enDT = new Entity("ssg_dailytracking");
            enDT.Attributes.Add("ssg_dailytrackingid", new Guid(destinationId));
            enDT.Attributes.Add("ssg_leavessequencenumber", iLeaveSequence + 1);
            enDT.Attributes.Add("ssg_leavessequenceincrementby", 1);
            _service.Update(enDT);
        }
        public void GenerateClientLogsManual(String parentId, String destinationId)
        {
            ConditionExpression condition1 = new ConditionExpression();
            condition1.AttributeName = "ssg_dailytrackingid";
            condition1.Operator = ConditionOperator.Equal;
            condition1.Values.Add(parentId);

            FilterExpression filter1 = new FilterExpression();
            filter1.Conditions.Add(condition1);

            QueryExpression query = new QueryExpression("ssg_clientlogmanualentry");
            query.ColumnSet.AddColumns("ssg_subject");
            query.ColumnSet.AddColumns("ssg_type");
            query.ColumnSet.AddColumns("ssg_details");

            query.Criteria.AddFilter(filter1);

            EntityCollection ecClientLogs = _service.RetrieveMultiple(query);

            if (ecClientLogs.Entities.Count > 0)
            {
                _tracingService.Trace("Count of Leaves: " + ecClientLogs.Entities.Count.ToString());
                foreach (var logs in ecClientLogs.Entities)
                {
                    Entity clientLog = new Entity("ssg_clientlogmanualentry");
                    clientLog.Attributes.Add("ssg_clientlogmanualentryid", logs.Id);

                    clientLog.Attributes.Add("ssg_dailytrackingid", new EntityReference("ssg_dailytracking", new Guid(destinationId)));
                    _service.Update(clientLog);
                }
            }
        }


        public void GenerateClientLogs(String parentId, String destinationId)
        {
            ConditionExpression condition1 = new ConditionExpression();
            condition1.AttributeName = "ssg_dailytrackingid";
            condition1.Operator = ConditionOperator.Equal;
            condition1.Values.Add(parentId);

            FilterExpression filter1 = new FilterExpression();
            filter1.Conditions.Add(condition1);

            QueryExpression query = new QueryExpression("ssg_clientlog");


            query.Criteria.AddFilter(filter1);

            EntityCollection ecClientLogs = _service.RetrieveMultiple(query);

            _tracingService.Trace("Count of Leaves: " + ecClientLogs.Entities.Count.ToString());
            if (ecClientLogs.Entities.Count > 0)
            {
                _tracingService.Trace("Count of Leaves: " + ecClientLogs.Entities.Count.ToString());
                foreach (var logs in ecClientLogs.Entities)
                {
                    Entity clientLog = new Entity("ssg_clientlog");
                    clientLog.Attributes.Add("ssg_clientlogid", logs.Id);

                    clientLog.Attributes.Add("ssg_dailytrackingid", new EntityReference("ssg_dailytracking", new Guid(destinationId)));
                    _service.Update(clientLog);
                }
            }
        }
    }
}
