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
    public class CreateUpdateDailyTracking : IPlugin
    {
        IOrganizationService _service;
        IPluginExecutionContext _context;
        public void Execute(IServiceProvider serviceProvider)
        {
            //Task.Delay(120000).Wait();
            _context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));
            //if (_context.InputParameters.Contains("Target") &&
            //            _context.InputParameters["Target"] is Entity)
            {
                // Obtain the target entity from the input parameters.  
                //Entity entity = (Entity)_context.InputParameters["Target"];

                // Obtain the organization service reference which you will need for  
                // web service calls.  
                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                _service = serviceFactory.CreateOrganizationService(_context.UserId);
                try
                {
                    
                    
                    List<Entity> lstDT = new List<Entity>();
                    List<String> lstUnitCell = new List<String>();
                    String sCTOCFilter = String.Empty;
                    var ssg_date = DateTime.Today;
                    #region


                    //ConditionExpression condition1 = new ConditionExpression();
                    //condition1.AttributeName = "name";
                    //condition1.Operator = ConditionOperator.Equal;
                    //condition1.Values.Add(_context.InputParameters["BusinessUnit"]);

                    //FilterExpression filter1 = new FilterExpression();
                    //filter1.Conditions.Add(condition1);

                    //QueryExpression query = new QueryExpression("businessunit");
                    //query.ColumnSet.AddColumns("businessunitid");
                    //query.Criteria.AddFilter(filter1);

                    //EntityCollection ecBU = _service.RetrieveMultiple(query);



                    #endregion
                    //if (ecBU.Entities.Count == 0)
                    {
                    

                    #region Fetch active Daily Tracking Record

                    var fetchDailyTracking = "<fetch distinct='false' mapping='logical' output-format='xml-platform' version='1.0'>" +
                                    "<entity name='ssg_dailytracking'>" +
                                    "<attribute name='ssg_name'/>" +
                                    "<attribute name='createdon'/>" +
                                    "<attribute name='ssg_date'/>" +
                                    "<attribute name='ssg_csnumber'/>" +
                                    "<attribute name='ssg_client'/>" +
                                    "<attribute name='ssg_celllocation'/>" +
                                    "<attribute name='ssg_dailytrackingid'/>" +
                                    "<attribute name='ssg_15minutechecks'/>" +
                                    "<attribute name='ssg_specialhandlingprotocols'/>" +
                                    "<attribute name='ssg_cgicrating'/>" +
                                    "<attribute name='ssg_completedcaseplan'/>" +
                                    "<attribute name='ssg_securitycautionsalertsnotes'/>" +
                                    "<order descending='true' attribute='ssg_date'/>" +
                                    "<filter type = 'and'>" +
                                    "<condition attribute = 'ssg_date' operator= 'yesterday'/>"+ 
                                    "<condition attribute = 'ssg_lastactive' operator= 'eq' value = '1'/>"+
                                    "<condition attribute = 'ssg_client' operator= 'not-null'/>"+
                                    "</filter >" +
                                    "</entity>" +
                                    "</fetch>";

                    EntityCollection ecDT = _service.RetrieveMultiple(new FetchExpression(fetchDailyTracking));
                   
                    #endregion



                    #region Fetch Client with Unit Cell

                    var fetchUnitCellWithClient = "<fetch distinct='false' mapping='logical' output-format='xml-platform' version='1.0'>" +
                                        "<entity name='ssg_cell'>" +
                                        "<attribute name='ssg_cellid'/>" +
                                        "<attribute name='ssg_name'/>" +
                                        "<attribute name='createdon'/>" +
                                        "<attribute name='ssg_celltype'/>" +
                                        "<attribute name='ssg_businessunit'/>" +
                                        "<attribute name='ssg_camera'/>" +
                                        "<attribute name='ssg_callbox'/>" +
                                        "<attribute name='ssg_unitcode'/>" +
                                        "<attribute name='ssg_cellcode'/>" +
                                        "<attribute name='ssg_clientid'/>" +
                                        "<order descending='false' attribute='ssg_businessunit'/>" +
                                        "<link-entity name='contact' alias='client' link-type='inner' visible='false' to='ssg_clientid' from='contactid'>" +
                                        "<attribute name='ssg_csnumber'/>" +
                                        "<attribute name='ssg_correctionalcentre'/>" +
                                        "<attribute name='ssg_populationdesignation'/>" +
                                        "<attribute name='ssg_dualbunkable'/>" +
                                        "<attribute name='ssg_indigenous'/>" +
                                        "<attribute name='ssg_mentalhealthneeds'/>" +
                                        "<attribute name='ssg_lastcontinuousperiodofseparateconfinement'/>" +
                                        "<filter type='and'>" +

                                        //"<condition attribute = 'ssg_correctionalcentre' operator= 'eq' value=<'"+ "07b1c3c1-7081-e811-8158-480fcff44541" +"'/>"+
                                        "</filter>"+
                                        "</link-entity>" +
                                        "</entity>" +
                                        "</fetch>";

                    EntityCollection ecUnitCellWithClient = _service.RetrieveMultiple(new FetchExpression(fetchUnitCellWithClient));
                    if (ecUnitCellWithClient.Entities.Count > 0)
                    {
                            
                        foreach (var ecClient in ecUnitCellWithClient.Entities)
                        {
                            var previousDT = ecDT.Entities.Where(p => p.GetAttributeValue<EntityReference>("ssg_client").Id == ecClient.GetAttributeValue<EntityReference>("ssg_clientid").Id).Select(p => p).ToList();

                            Entity enDT = new Entity("ssg_dailytracking");
                            enDT.Attributes.Add("ssg_name", ssg_date.ToShortDateString()+ " " + ssg_date.ToShortTimeString());
                            enDT.Attributes.Add("ssg_leavessequenceincrementby", 1);
                            enDT.Attributes.Add("ssg_leavessequencenumber", 1);
                                enDT.Attributes.Add("statuscode", new OptionSetValue(1));
                                enDT.Attributes.Add("statecode", new OptionSetValue(0));

                                enDT.Attributes.Add("ssg_client", ecClient.GetAttributeValue<EntityReference>("ssg_clientid"));
                                enDT.Attributes.Add("ssg_unitcell", ecClient.GetAttributeValue<String>("ssg_unitcode"));
                                enDT.Attributes.Add("ssg_celllocation", ecClient.GetAttributeValue<String>("ssg_cellcode"));

                                if (ecClient.Contains("client.ssg_csnumber"))
                                    enDT.Attributes.Add("ssg_csnumber",ecClient.GetAttributeValue<AliasedValue>("client.ssg_csnumber").Value.ToString());
                            if(ecClient.Contains("client.ssg_correctionalcentre"))
                                enDT.Attributes.Add("ssg_correctionalcentre", (EntityReference)(ecClient.GetAttributeValue<AliasedValue>("client.ssg_correctionalcentre").Value));
                            
                            enDT.Attributes.Add("ssg_cell", new EntityReference("ssg_cell", ecClient.GetAttributeValue<Guid>("ssg_cellid")));
                            enDT.Attributes.Add("ssg_date", ssg_date);
                            if (ecClient.Contains("client.ssg_populationdesignation") && ecClient.GetAttributeValue<AliasedValue>("client.ssg_populationdesignation").Value.ToString() != String.Empty)
                            {
                                if (ecClient.GetAttributeValue<AliasedValue>("client.ssg_populationdesignation").Value.ToString().Substring(0, 2) == "GP")
                                    enDT.Attributes.Add("ssg_popdesignation", "GP");
                                if (ecClient.GetAttributeValue<AliasedValue>("client.ssg_populationdesignation").Value.ToString().Substring(0, 2) == "PC")
                                    enDT.Attributes.Add("ssg_popdesignation", "PC");
                            }

                            if (ecClient.Contains("client.ssg_mentalhealthneeds"))
                                enDT.Attributes.Add("ssg_mentalhealthneeds", (OptionSetValue)ecClient.GetAttributeValue<AliasedValue>("client.ssg_mentalhealthneeds").Value);

                                if (ecClient.Contains("client.ssg_indigenous") && (bool)ecClient.GetAttributeValue<AliasedValue>("client.ssg_indigenous").Value == true)
                                    enDT.Attributes.Add("ssg_characterindigenous", new OptionSetValue(867670000));
                                else if (ecClient.Contains("client.ssg_indigenous") && (bool)ecClient.GetAttributeValue<AliasedValue>("client.ssg_indigenous").Value == false)
                                    enDT.Attributes.Add("ssg_characterindigenous", new OptionSetValue(867670001));

                                if (ecClient.Contains("client.ssg_dualbunkable"))
                                enDT.Attributes.Add("ssg_dualbunkable", (OptionSetValue)ecClient.GetAttributeValue<AliasedValue>("client.ssg_dualbunkable").Value);

                            if (ecClient.Contains("client.ssg_lastcontinuousperiodofseparateconfinement") && ecClient.GetAttributeValue<AliasedValue>("client.ssg_lastcontinuousperiodofseparateconfinement") != null)
                                enDT.Attributes.Add("ssg_continuousseparateconfinementid", (EntityReference)(ecClient.GetAttributeValue<AliasedValue>("client.ssg_lastcontinuousperiodofseparateconfinement").Value));
                            if (ecClient.Contains("ssg_celltype"))
                                enDT.Attributes.Add("ssg_celltype", ecClient.GetAttributeValue<OptionSetValue>("ssg_celltype"));
                            if (previousDT.Count > 0)
                            {
                                //enDT.Attributes.Add("ssg_clonefromdailytrackingid", new EntityReference("ssg_dailytracking", previousDT[0].GetAttributeValue<Guid>("ssg_dailytrackingid")));
                                if (previousDT[0].Contains("ssg_15minutechecks"))
                                    enDT.Attributes.Add("ssg_15minutechecks", previousDT[0].GetAttributeValue<OptionSetValue>("ssg_15minutechecks"));
                                if (previousDT[0].Contains("ssg_specialhandlingprotocols"))
                                    enDT.Attributes.Add("ssg_specialhandlingprotocols", previousDT[0].GetAttributeValue<OptionSetValue>("ssg_specialhandlingprotocols"));
                                if (previousDT[0].Contains("ssg_cgicrating"))
                                    enDT.Attributes.Add("ssg_cgicrating",previousDT[0].GetAttributeValue<OptionSetValue>("ssg_cgicrating"));
                                if (previousDT[0].Contains("ssg_completedcaseplan"))
                                    enDT.Attributes.Add("ssg_completedcaseplan", previousDT[0].GetAttributeValue<OptionSetValue>("ssg_completedcaseplan"));
                                if (previousDT[0].Contains("ssg_securitycautionsalertsnotes"))
                                    enDT.Attributes.Add("ssg_securitycautionsalertsnotes", previousDT[0].GetAttributeValue<String>("ssg_securitycautionsalertsnotes"));                                

                            }
                           

                            lstDT.Add(enDT);
                            
                            sCTOCFilter = sCTOCFilter + "<value>" + ecClient.GetAttributeValue<EntityReference>("ssg_clientid").Id.ToString() + "</value>";
                        }
                    }

                        #endregion

                        #region Fetch Active CTOC Record

                        var fetchActiveCTOC = String.Empty;
                    if (sCTOCFilter!= String.Empty)
                         fetchActiveCTOC = "<fetch distinct='true' mapping='logical' output-format='xml-platform' version='1.0'>" +
                                                "<entity name='ssg_separateconfinementperiod'>" +
                                                "<attribute name='ssg_separateconfinementperiodid'/>" +
                                                "<attribute name='ssg_name'/>" +
                                                "<attribute name='ssg_client'/>" +
                                                "<attribute name='ssg_csnumber'/>" +
                                                "<attribute name='ssg_correctioncentre'/>" +
                                                "<filter type='and'>" +
                                                "<condition attribute='statecode' value='0' operator='eq'/>" +
                                                "<condition attribute='ssg_client' operator='not-null'/>"+
                                                "<condition attribute='ssg_client' operator='not-in'>" +
                                                sCTOCFilter +

                                                "</condition>" +
                                                "</filter>" +
                                                "<link-entity name='contact' from='contactid' to='ssg_client' visible='false' link-type='outer' alias='client'>" +
                                                "<attribute name='ssg_populationdesignation'/>" +
                                                "<attribute name='ssg_dualbunkable'/>" +
                                                "<attribute name='ssg_indigenous'/>" +
                                                "<attribute name='ssg_mentalhealthneeds'/>" +
                                                "<attribute name='ssg_celllocation'/>" +
                                                "</link-entity>" +
                                                "</entity>" +
                                                "</fetch>";
                       else
                            fetchActiveCTOC = "<fetch distinct='true' mapping='logical' output-format='xml-platform' version='1.0'>" +
                                                   "<entity name='ssg_separateconfinementperiod'>" +
                                                   "<attribute name='ssg_separateconfinementperiodid'/>" +
                                                   "<attribute name='ssg_name'/>" +
                                                   "<attribute name='ssg_client'/>" +
                                                   "<attribute name='ssg_csnumber'/>" +
                                                   "<attribute name='ssg_correctioncentre'/>" +
                                                   "<filter type='and'>" +
                                                   "<condition attribute='statecode' value='0' operator='eq'/>" +
                                                   "<condition attribute='ssg_client' operator='not-null'/>" +                                                   
                                                   "</filter>" +
                                                   "<link-entity name='contact' from='contactid' to='ssg_client' visible='false' link-type='outer' alias='client'>" +
                                                   "<attribute name='ssg_populationdesignation'/>" +
                                                   "<attribute name='ssg_dualbunkable'/>" +
                                                   "<attribute name='ssg_indigenous'/>" +
                                                    "<attribute name='ssg_mentalhealthneeds'/>" +
                                                    "<attribute name='ssg_celllocation'/>" +
                                                   "</link-entity>" +
                                                   "</entity>" +
                                                   "</fetch>";

                        EntityCollection ecActiveCTOC = _service.RetrieveMultiple(new FetchExpression(fetchActiveCTOC));

                    if (ecActiveCTOC.Entities.Count > 0)
                    {
                        foreach (var ecClient in ecActiveCTOC.Entities)
                        {
                            var previousDT = ecDT.Entities.Where(p => p.GetAttributeValue<EntityReference>("ssg_client").Id == ecClient.GetAttributeValue<EntityReference>("ssg_client").Id).Select(p => p).ToList();

                            Entity enDT = new Entity("ssg_dailytracking");
                                enDT.Attributes.Add("ssg_leavessequenceincrementby", 1);
                                enDT.Attributes.Add("ssg_leavessequencenumber", 1);
                                enDT.Attributes.Add("statuscode", new OptionSetValue(1));
                                enDT.Attributes.Add("statecode", new OptionSetValue(0));
                                enDT.Attributes.Add("ssg_name", ssg_date.ToShortDateString() + " " + ssg_date.ToShortTimeString());
                            enDT.Attributes.Add("ssg_client", ecClient.GetAttributeValue<EntityReference>("ssg_client"));
                            if(ecClient.Contains("ssg_csnumber"))
                                enDT.Attributes.Add("ssg_csnumber", ecClient.GetAttributeValue<String>("ssg_csnumber"));
                            if (ecClient.Contains("ssg_correctioncentre"))
                                enDT.Attributes.Add("ssg_correctionalcentre", ecClient.GetAttributeValue<EntityReference>("ssg_correctioncentre"));
                            
                            enDT.Attributes.Add("ssg_date", ssg_date);
                            if (ecClient.Contains("client.ssg_populationdesignation") && ecClient.GetAttributeValue<AliasedValue>("client.ssg_populationdesignation").Value.ToString() != String.Empty)
                            {
                                if(ecClient.GetAttributeValue<AliasedValue>("client.ssg_populationdesignation").Value.ToString().Substring(0,2) == "GP")
                                   enDT.Attributes.Add("ssg_popdesignation", "GP");
                                if (ecClient.GetAttributeValue<AliasedValue>("client.ssg_populationdesignation").Value.ToString().Substring(0, 2) == "PC")
                                     enDT.Attributes.Add("ssg_popdesignation", "PC");
                            }

                                if (ecClient.Contains("client.ssg_celllocation"))
                                    enDT.Attributes.Add("ssg_celllocation", (String)ecClient.GetAttributeValue<AliasedValue>("client.ssg_celllocation").Value);

                                if (ecClient.Contains("client.ssg_dualbunkable"))
                                    enDT.Attributes.Add("ssg_dualbunkable", (OptionSetValue)ecClient.GetAttributeValue<AliasedValue>("client.ssg_dualbunkable").Value);

                                if (ecClient.Contains("client.ssg_mentalhealthneeds"))
                                    enDT.Attributes.Add("ssg_mentalhealthneeds", (OptionSetValue)ecClient.GetAttributeValue<AliasedValue>("client.ssg_mentalhealthneeds").Value);

                                if (ecClient.Contains("client.ssg_indigenous") && (bool)ecClient.GetAttributeValue<AliasedValue>("client.ssg_indigenous").Value == true)
                                    enDT.Attributes.Add("ssg_characterindigenous", new OptionSetValue(867670000));
                                else if (ecClient.Contains("client.ssg_indigenous") && (bool)ecClient.GetAttributeValue<AliasedValue>("client.ssg_indigenous").Value == false)
                                    enDT.Attributes.Add("ssg_characterindigenous", new OptionSetValue(867670001));



                                if (ecClient.Contains("ssg_separateconfinementperiodid") && ecClient.GetAttributeValue<Guid>("ssg_separateconfinementperiodid") !=null)
                                enDT.Attributes.Add("ssg_continuousseparateconfinementid", new EntityReference("ssg_separateconfinementperiod", ecClient.GetAttributeValue<Guid>("ssg_separateconfinementperiodid")));


                                if (previousDT.Count>0)
                            {
                                //enDT.Attributes.Add("ssg_clonefromdailytrackingid", new EntityReference("ssg_dailytracking", previousDT[0].GetAttributeValue<Guid>("ssg_dailytrackingid")));
                                if (previousDT[0].Contains("ssg_15minutechecks"))
                                    enDT.Attributes.Add("ssg_15minutechecks", previousDT[0].GetAttributeValue<OptionSetValue>("ssg_15minutechecks"));
                                if (previousDT[0].Contains("ssg_specialhandlingprotocols"))
                                    enDT.Attributes.Add("ssg_specialhandlingprotocols", previousDT[0].GetAttributeValue<OptionSetValue>("ssg_specialhandlingprotocols"));
                                if (previousDT[0].Contains("ssg_cgicrating"))
                                    enDT.Attributes.Add("ssg_cgicrating", previousDT[0].GetAttributeValue<OptionSetValue>("ssg_cgicrating"));
                                if (previousDT[0].Contains("ssg_completedcaseplan"))
                                    enDT.Attributes.Add("ssg_completedcaseplan", previousDT[0].GetAttributeValue<OptionSetValue>("ssg_completedcaseplan"));
                                if (previousDT[0].Contains("ssg_securitycautionsalertsnotes"))
                                    enDT.Attributes.Add("ssg_securitycautionsalertsnotes", previousDT[0].GetAttributeValue<String>("ssg_securitycautionsalertsnotes"));

                            }
                                

                            lstDT.Add(enDT);
                           
                        }
                    }

                    #endregion
                    lstDT = lstDT.GroupBy(p => p.GetAttributeValue<EntityReference>("ssg_client").Id).Select(g => g.First()).ToList();

                    }


                    #region Fetch  Unit Cell without client
                    //if (_context.InputParameters["UnoccupiedUnits"].ToString() == "true")
                    {
                        var fetchUnitCell = "<fetch distinct='false' mapping='logical' output-format='xml-platform' version='1.0'>" +
                                            "<entity name='ssg_cell'>" +
                                            "<attribute name='ssg_cellid'/>" +
                                            "<attribute name='ssg_name'/>" +
                                            "<attribute name='createdon'/>" +
                                            "<attribute name='ssg_celltype'/>" +
                                            "<attribute name='ssg_businessunit'/>" +
                                            "<attribute name='ssg_camera'/>" +
                                            "<attribute name='ssg_callbox'/>" +
                                            "<attribute name='ssg_unitcode'/>" +
                                            "<attribute name='ssg_cellcode'/>" +
                                            "<order descending='false' attribute='ssg_businessunit'/>" +
                                            "<link-entity name='contact' alias='aa' link-type='outer' to='ssg_clientid' from='contactid'/>" +
                                            "<filter type='and'>" +
                                            "<condition attribute='contactid' operator='null' entityname='aa'/>" +
                                            "<condition attribute='statecode' operator='eq' value='0'/>"+
                                                "</filter>" +
                                            "</entity>" +
                                            "</fetch>";

                        EntityCollection ecUnitCell = _service.RetrieveMultiple(new FetchExpression(fetchUnitCell));
                        if (ecUnitCell.Entities.Count > 0)
                        {
                            foreach (var ecClient in ecUnitCell.Entities)
                            {


                                Entity enDT = new Entity("ssg_dailytracking");
                                enDT.Attributes.Add("ssg_name", ssg_date.ToShortDateString() + " " + ssg_date.ToShortTimeString());
                                enDT.Attributes.Add("ssg_leavessequenceincrementby", 0);
                                enDT.Attributes.Add("ssg_cell", new EntityReference("ssg_cell", ecClient.GetAttributeValue<Guid>("ssg_cellid")));
                                enDT.Attributes.Add("ssg_date", ssg_date);
                                if (ecClient.Contains("ssg_celltype"))
                                    enDT.Attributes.Add("ssg_celltype", ecClient.GetAttributeValue<OptionSetValue>("ssg_celltype"));
                                if (ecClient.Contains("ssg_businessunit"))
                                    enDT.Attributes.Add("ssg_correctionalcentre", ecClient.GetAttributeValue<EntityReference>("ssg_businessunit"));
                                enDT.Attributes.Add("ssg_unitcell", ecClient.GetAttributeValue<String>("ssg_unitcode"));
                                enDT.Attributes.Add("ssg_celllocation", ecClient.GetAttributeValue<String>("ssg_cellcode"));
                                enDT.Attributes.Add("statuscode", new OptionSetValue(1));
                                enDT.Attributes.Add("statecode", new OptionSetValue(0));

                                lstDT.Add(enDT);
                            }
                        }
                    }

                    #endregion

                   
                    #region Generate Daily Tracking

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

                    foreach (var enDT in lstDT)
                    {
                        _service.Create(enDT);
                        //CreateRequest createRequest = new CreateRequest { Target = entity };
                        //request.Requests.Add(createRequest);
                    }

                    //ExecuteMultipleResponse response = (ExecuteMultipleResponse)_service.Execute(request);


                    #endregion


                    var fetchClient = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
  "<entity name='contact'>" +
    "<attribute name='fullname' />" +
    "<attribute name='telephone1' />" +
    "<attribute name='contactid' />" +
    "<order attribute='ssg_csnumber' descending='false' />" +
    "<filter type='and'>" +
      "<filter type='or'>" +
       "<condition attribute='ssg_deactivatedt' operator='eq' value='867670000' />" +
    "<condition attribute='ssg_regeneratedt' operator='eq' value='867670000' />" +
    "<condition attribute='ssg_clonedt' operator='eq' value='867670000' />" +
      "</filter>" +
    "</filter>" +
  "</entity>" +
"</fetch>";
                    EntityCollection ecClientClearUpdates = _service.RetrieveMultiple(new FetchExpression(fetchClient));
                    if (ecClientClearUpdates.Entities.Count > 0)
                    {
                        foreach (var client in ecClientClearUpdates.Entities)
                        {
                            Entity contact = new Entity("contact");
                            contact.Attributes.Add("ssg_deactivatedt", new OptionSetValue(867670001));
                            contact.Attributes.Add("ssg_regeneratedt", new OptionSetValue(867670001));
                            contact.Attributes.Add("ssg_clonedt", new OptionSetValue(867670001));
                            contact.Attributes.Add("contactid", client.Id);
                            _service.Update(contact);
                        }
                    }

                    UpdateRosterRefreshLog("Completed", "");


                }
                catch (Exception e)
                {
                    throw new InvalidPluginExecutionException(e + "CreateUpdateDailyTracking Plugin error: "+ e.Message);
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
