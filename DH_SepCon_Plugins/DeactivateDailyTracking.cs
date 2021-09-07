using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using DH_SepCon_Plugins.Helper;
using Microsoft.Xrm.Sdk.Messages;


namespace DH_SepCon_Plugins
{
    public class DeactivateDailyTracking:IPlugin
    {
        IOrganizationService _service;
        IPluginExecutionContext _context;
        public void Execute(IServiceProvider serviceProvider)
        {
            _context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));
            //if (_context.InputParameters.Contains("Target") &&
              //          _context.InputParameters["Target"] is Entity)
            {

                
                

                // Obtain the organization service reference which you will need for  
                // web service calls.  
                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                _service = serviceFactory.CreateOrganizationService(_context.UserId);
                try
                {


                    List<DailyTracking> lstDailyTracking = new List<DailyTracking>();
                    List<Entity> lstDT = new List<Entity>();
                    List<String> lstUnitCell = new List<String>();
                    String sCTOCFilter = String.Empty;


                    #region Fetch active Daily Tracking Record

                    var fetchDailyTracking = "<fetch distinct='false' mapping='logical' output-format='xml-platform' version='1.0'>" +
                                    "<entity name='ssg_dailytracking'>" +
                                    
                                    "<attribute name='ssg_dailytrackingid'/>" +                                    
                                    "<filter type = 'and'>" +
                                    "<condition attribute = 'statecode' value ='0' operator= 'eq'/>" +
                                    "</filter >" +
                                    "</entity>" +
                                    "</fetch>";

                    EntityCollection ecDT = _service.RetrieveMultiple(new FetchExpression(fetchDailyTracking));
                    
                    #region Update Status
                    foreach (var enDT in ecDT.Entities)
                    {
                        Entity updateDT = new Entity("ssg_dailytracking");
                        //enDT.Attributes.Add("ssg_dailytrackingid", enDT.GetAttributeValue<Guid>("ssg_dailytrackingid"));
                        updateDT.Attributes.Add("ssg_lastactive", true);
                        updateDT.Attributes.Add("ssg_dailytrackingid", enDT.Id);
                        //Complete- 2
                        updateDT.Attributes.Add("statuscode", new OptionSetValue(2));
                        updateDT.Attributes.Add("statecode", new OptionSetValue(1));
                        _service.Update(updateDT);
                    }
                    #endregion

                   
                    #endregion
                                       
                    #region Update Daily Tracking

                    ExecuteMultipleRequest request = new ExecuteMultipleRequest()
                    {
                        // Assign settings that define execution behavior: continue on error, return responses.
                        Settings = new ExecuteMultipleSettings()
                        {
                            ContinueOnError = true,
                            ReturnResponses = true
                        },

                        // Create an empty organization request collection.
                        Requests = new OrganizationRequestCollection()
                    };

                    //foreach (var enDT in ecDT.Entities)
                    //{
                    //    _service.Update(enDT);
                    //    //CreateRequest createRequest = new CreateRequest { Target = entity };
                    //    //request.Requests.Add(createRequest);
                    //}

                    //ExecuteMultipleResponse response = (ExecuteMultipleResponse)_service.Execute(request);


                    #endregion


                    _context.OutputParameters["DeactivationStatus"] = "Completed";

                    //var executeAction = _service.Execute(new OrganizationRequest()
                    //{
                    //    RequestName = "ssg_DailyTrackingDailyJobCreateNewDailyTrackingrecords",
                    //});







                }
                catch (Exception e)
                {
                    UpdateRosterRefreshLog("Failed", e.Message);
                    throw new InvalidPluginExecutionException(e + "PostCreateInmateAssessment Plugin error");
                }

            }
        }


        public void UpdateRosterRefreshLog(String status, String details)
        {
            ConditionExpression condition1 = new ConditionExpression();
            condition1.AttributeName = "statecode";
            condition1.Operator = ConditionOperator.Equal;
            condition1.Values.Add(0);

            FilterExpression filter1 = new FilterExpression();
            filter1.Conditions.Add(condition1);

            QueryExpression query = new QueryExpression("ssg_rosterrefreshlog");
            query.ColumnSet.AddColumns("ssg_rosterrefreshlogid");
            query.Criteria.AddFilter(filter1);

            EntityCollection ecRRL = _service.RetrieveMultiple(query);

            if (ecRRL.Entities.Count > 0)
            {
                Entity enRRL = new Entity("ssg_rosterrefreshlog");
                enRRL.Attributes.Add("ssg_rosterrefreshlogid", ecRRL.Entities[0].Id);
                enRRL.Attributes.Add("ssg_processcompletiontime", DateTime.Now);
                enRRL.Attributes.Add("ssg_processstatus", status);
                enRRL.Attributes.Add("ssg_processdetails", details);
                enRRL.Attributes.Add("statuscode", new OptionSetValue(2));
                enRRL.Attributes.Add("statecode", new OptionSetValue(1));

                _service.Update(enRRL);
            }


        }

    }
}
