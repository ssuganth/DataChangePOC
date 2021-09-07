using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using DH_SepCon_Plugins.Helper;
using Microsoft.Xrm.Sdk.Messages;
using System.Threading;

namespace DH_SepCon_Plugins
{
    public class RosterRefresh : IPlugin
    {
        IOrganizationService _service;
        IPluginExecutionContext _context;
        List<Entity> lstCurrentDT = new List<Entity>();
        List<Entity> lstNewDT = new List<Entity>();
        
        List<Entity> lstUpdatedClient = new List<Entity>();
        ITracingService trace;
        String _sPrevDTFilter = String.Empty;

        public void Execute(IServiceProvider serviceProvider)
        {
            _context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));



            // Obtain the organization service reference which you will need for  
            // web service calls.  
            IOrganizationServiceFactory serviceFactory =
                (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            _service = serviceFactory.CreateOrganizationService(_context.UserId);
            trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            try
            {

                String sDTFilter = String.Empty;

                #region Fetch all modified Client

                var fetchModifiedClients = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
                                        "<entity name='contact'>" +
                                        "<attribute name='fullname'/>" +
                                        "<attribute name='telephone1'/>" +
                                        "<attribute name='contactid'/>" +
                                        "<attribute name='ssg_correctionalcentre'/>" +
                                        "<attribute name='ssg_celllocation'/>" +
                                        "<attribute name='ssg_cellid'/>" +
                                        "<attribute name='ssg_lastcontinuousperiodofseparateconfinement'/>" +
                                        "<attribute name='ssg_clientoutofconfinement'/>" +
                                        "<attribute name='ssg_clientinconfinement'/>" +
                                        "<attribute name='ssg_clientcellchanged'/>" +
                                        "<attribute name='ssg_clientlocationchanged'/>" +
                                        "<attribute name='ssg_celllocation'/>" +
                                        "<attribute name='ssg_dailytrackingid'/>" +
                                        "<attribute name='ssg_lastdailytrackingid'/>" +
                                        "<attribute name='ssg_populationdesignation'/>" +
                                        "<attribute name='ssg_dualbunkable'/>" +
                                        "<attribute name='ssg_indigenous'/>" +
                                        "<attribute name='ssg_mentalhealthneeds'/>" +
                                        "<attribute name='ssg_csnumber'/>" +
                                         "<attribute name='ssg_clonedt'/>" +
                                          "<attribute name='ssg_regeneratedt'/>" +
                                          "<attribute name='ssg_deactivatedt'/>" +
                                          "<attribute name='ssg_backdateddt'/>" +
                                        "<order attribute='fullname' descending='false'/>" +
                                        "<filter type='and'>" +
                                        "<filter type='or'>" +
                                        "<condition attribute='ssg_regeneratedt' operator='eq' value='867670000'/>" +
                                        "<condition attribute='ssg_clonedt' operator='eq' value='867670000'/>" +
                                        "<condition attribute='ssg_deactivatedt' operator='eq' value='867670000'/>" +
                                        "<condition attribute='ssg_backdateddt' operator='eq' value='867670000'/>" +
                                        "</filter>" +
                                        "</filter>" +
                                        "<link-entity name='ssg_separateconfinementperiod' from='ssg_separateconfinementperiodid' to='ssg_lastcontinuousperiodofseparateconfinement' visible='false' link-type='outer' alias='CTOC'>" +
                                        "<attribute name='statecode'/>" +
                                        "<attribute name='ssg_confinementstartdate'/>"+
                                        "</link-entity>" +
                                        "<link-entity name='ssg_dailytracking' from='ssg_dailytrackingid' to='ssg_dailytrackingid' visible='false' link-type='outer' alias='DT'>" +
                                        "<attribute name='ssg_unitcell'/>" +
                                        "<attribute name='ssg_date'/>" +
                                        "<attribute name='ssg_dailytrackingid'/>" +
                                        "<attribute name='ssg_cell'/>" +
                                        "<attribute name='ssg_timeoutofcell'/>" +
                                        "<attribute name='ssg_specialhandlingprotocols'/>" +
                                        "<attribute name='ssg_securitycautionsalertsnotes'/>" +
                                        "<attribute name='ssg_popdesignation'/>" +

                                        "<attribute name='ssg_scdailynotes'/>" +
                                        "<attribute name='ssg_mentalhealthneeds'/>" +
                                        "<attribute name='ssg_mhdwreview'/>" +
                                        "<attribute name='ssg_medicalconditions'/>" +
                                        "<attribute name='ssg_meaningfulhumancontacthrs'/>" +
                                        "<attribute name='ssg_managerreviewtime'/>" +
                                        "<attribute name='ssg_managerreviewcompleted'/>" +
                                        "<attribute name='ssg_lastactive'/>" +
                                        "<attribute name='ssg_characterindigenous'/>" +
                                        "<attribute name='ssg_iaclassificationfromclient'/>" +
                                        "<attribute name='ssg_hqreview'/>" +
                                        "<attribute name='ssg_healthcareconsulttime'/>" +
                                        "<attribute name='ssg_healthcareconsultcompleted'/>" +

                                        "<attribute name='ssg_hadcellmatetoday'/>" +
                                        "<attribute name='ssg_dualbunkable'/>" +
                                        "<attribute name='ssg_date'/>" +
                                        "<attribute name='ssg_name'/>" +
                                        "<attribute name='ssg_csnumber'/>" +
                                        "<attribute name='ssg_correctionalcentre'/>" +
                                        "<attribute name='ssg_continuousseparateconfinementid'/>" +
                                        "<attribute name='ssg_consecutivedaysconfined'/>" +
                                        "<attribute name='ssg_separateconfinement'/>" +
                                        "<attribute name='ssg_clonetodailytrackingid'/>" +
                                        "<attribute name='ssg_clonefromdailytrackingid'/>" +
                                        "<attribute name='ssg_client'/>" +
                                        "<attribute name='ssg_cgicrating'/>" +
                                        "<attribute name='ssg_celltype'/>" +
                                        "<attribute name='ssg_celllocation'/>" +
                                        "<attribute name='ssg_cellcamnumber'/>" +
                                        "<attribute name='ssg_cellcallbox'/>" +
                                        "<attribute name='ssg_completedcaseplan'/>" +
                                        "<attribute name='ssg_15minutechecks'/>" +
                                        "<filter type='and'>" +
                                        "<condition attribute='statecode' operator='eq' value='0'/>" +
                                        "</filter>" +
                                        "</link-entity>" +                                        

                                        "<link-entity name='ssg_dailytracking' from='ssg_dailytrackingid' to='ssg_lastdailytrackingid' visible='false' link-type='outer' alias='LDT'>" +
                                        "<attribute name='ssg_unitcell'/>" +
                                        "<attribute name='ssg_date'/>" +
                                        "<attribute name='ssg_dailytrackingid'/>" +
                                        "<attribute name='ssg_timeoutofcell'/>" +
                                        "<attribute name='ssg_specialhandlingprotocols'/>" +
                                        "<attribute name='ssg_securitycautionsalertsnotes'/>" +
                                        "<attribute name='ssg_mentalhealthneeds'/>" +
                                        "<attribute name='ssg_mhdwreview'/>" +
                                        "<attribute name='ssg_medicalconditions'/>" +
                                        "<attribute name='ssg_meaningfulhumancontacthrs'/>" +
                                        "<attribute name='ssg_managerreviewtime'/>" +
                                        "<attribute name='ssg_managerreviewcompleted'/>" +
                                        "<attribute name='ssg_lastactive'/>" +
                                        "<attribute name='ssg_characterindigenous'/>" +
                                        "<attribute name='ssg_iaclassificationfromclient'/>" +
                                        "<attribute name='ssg_hqreview'/>" +
                                        "<attribute name='ssg_healthcareconsulttime'/>" +
                                        "<attribute name='ssg_healthcareconsultcompleted'/>" +
                                        "<attribute name='ssg_hadcellmatetoday'/>" +
                                        "<attribute name='ssg_dualbunkable'/>" +
                                        "<attribute name='ssg_cgicrating'/>" +
                                        "<attribute name='ssg_completedcaseplan'/>" +
                                        "<attribute name='ssg_15minutechecks'/>" +
                                        "</link-entity>" +
                                        "<link-entity name='ssg_cell' alias='unitcell' link-type='outer' to='ssg_cellid' from='ssg_cellid'>" +
 "<attribute name='ssg_unitcode'/>" +
  "<attribute name='ssg_cellcode'/>" +
"</link-entity>" +
                                        "</entity>" +
                                        "</fetch>";
                EntityCollection ecModifiedClients = _service.RetrieveMultiple(new FetchExpression(fetchModifiedClients));
                #endregion

                if (ecModifiedClients.Entities.Count > 0)
                {
                    #region BackDatedDT

                    //Fetch all the client where backdatedDT flag = true
                    List<Entity> lstBackDatedDT = ecModifiedClients.Entities.Where(p =>
                                                             p.Contains("ssg_backdateddt")
                                                             && p.GetAttributeValue<OptionSetValue>("ssg_backdateddt").Value == 867670000).Select(p => p).ToList();
                    if (lstBackDatedDT.Count > 0)
                    {

                        //trace.Trace("BackDated DT " + lstBackDatedDT.Count.ToString());
                        foreach (var client in lstBackDatedDT)
                        {
                            BackDatedDT(client);

                            ClearClientFlag(client, "Backdated");
                            client.Attributes.Add("Considered", true);
                        }

                    }
                    #endregion

                    //Fetch all the client where deactivate DT = true
                    List<Entity> lstDeactivateDT = ecModifiedClients.Entities.Where(p => p.Contains("ssg_deactivatedt")
                                                            && p.GetAttributeValue<OptionSetValue>("ssg_deactivatedt").Value == 867670000).Select(p => p).ToList();

                    if (lstDeactivateDT.Count > 0)
                    {
                        trace.Trace("Deactivate DT " + lstDeactivateDT.Count.ToString());
                        foreach (var client in lstDeactivateDT)
                        {
                            if (client.Contains("ssg_dailytrackingid"))
                                DeactivateDT(client, client.GetAttributeValue<EntityReference>("ssg_dailytrackingid").Id);
                            ClearClientFlag(client,"Deactivate");
                            client.Attributes.Add("Considered", true);
                        }

                    }


                    //Fetch all the client where deactivate DT is null or false and regenerate dt = true
                    List<Entity> lstRegenerateDT = ecModifiedClients.Entities.Where(p => (!p.Contains("ssg_deactivatedt")
                                                             || (p.Contains("ssg_deactivatedt") && p.GetAttributeValue<OptionSetValue>("ssg_deactivatedt").Value == 867670001))                                                             
                                                             && p.Contains("ssg_regeneratedt")
                                                             && p.GetAttributeValue<OptionSetValue>("ssg_regeneratedt").Value == 867670000).Select(p => p).ToList();

                    if (lstRegenerateDT.Count > 0)
                    {
                        trace.Trace("Regenerate DT " + lstRegenerateDT.Count.ToString());
                        foreach (var client in lstRegenerateDT)
                        {
                            if (client.Contains("ssg_dailytrackingid"))
                                DeactivateDT(client, client.GetAttributeValue<EntityReference>("ssg_dailytrackingid").Id);

                            GenerateNewDT(client, false,"Regenerate", DateTime.Now);
                            ClearClientFlag(client,"Regenerate");
                            if(!client.Contains("Considered"))
                                client.Attributes.Add("Considered", true);
                            else
                            {
                                client["Considered"] = true;
                            }
                        }

                    }
                }
                else
                {
                    trace.Trace("No Clients are modified");
                }
               

                DeactivateDailyTracking();
                FetchChangedUnitCell();
                
                GenerateRecords();

                CreateRosterRefreshLog("Completed", null);
                
            }
            catch (Exception e)
            {

               

                throw new InvalidPluginExecutionException(e + "PostCreateInmateAssessment Plugin error");
            }


        }


        public void FetchChangedUnitCell()
        {
            var fetchDT = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
                                "<entity name='ssg_dailytracking'>" +
                                "<attribute name='ssg_dailytrackingid' />" +
                                "<order attribute='ssg_date' descending='true' />" +
                                "<filter type='and'>" +
                                "<condition attribute='statecode' operator='eq' value='0' />" +
                                "</filter>" +
                                "<link-entity name='ssg_cell' from='ssg_cellid' to='ssg_cell' link-type='inner' alias='aa'>" +
                                "<filter type='and'>" +
                                "<condition attribute='ssg_celllocationupdated' operator='eq' value='1' />" +
                                "</filter>" +
                                "</link-entity>" +
                                "</entity>" +
                                "</fetch>";

            EntityCollection ecModifiedDT = _service.RetrieveMultiple(new FetchExpression(fetchDT));
            trace.Trace("Active Daily Tracking with an update on Designated Cell Location: " + ecModifiedDT.Entities.Count.ToString());

            if (ecModifiedDT.Entities.Count > 0)
            {

                foreach (var DT in ecModifiedDT.Entities)
                {
                    DeactivateDT(DT, DT.GetAttributeValue<Guid>("ssg_dailytrackingid"));
                }
            }
            DeactivateDailyTracking();

            var fetchUnitCell = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
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
                                        "<attribute name='statuscode'/>" +
                                        "<attribute name='statecode'/>" +
                                        "<attribute name='overriddencreatedon'/>" +
                                        "<attribute name='ownerid'/>" +
                                        "<attribute name='modifiedon'/>" +
                                        "<attribute name='modifiedonbehalfby'/>" +
                                        "<attribute name='modifiedby'/>" +
                                        "<attribute name='createdonbehalfby'/>" +
                                        "<attribute name='createdby'/>" +
                                        "<attribute name='ssg_clientid'/>" +
                                        "<attribute name='ssg_celllocationupdated'/>" +
                                        "<order attribute='ssg_businessunit' descending='false'/>" +
                                        "<filter type='and'>" +
                                          "<condition attribute='ssg_celllocationupdated' operator='eq' value='1'/>" +
                                        "</filter>" +
                                      "</entity>" +
                                    "</fetch>";

            EntityCollection ecModifiedUnitCell = _service.RetrieveMultiple(new FetchExpression(fetchUnitCell));

            trace.Trace("Clear Designated Unit 'Cell location Updated' flag : " + ecModifiedUnitCell.Entities.Count.ToString());
            if (ecModifiedUnitCell.Entities.Count > 0)
            {
                foreach (var UnitCell in ecModifiedUnitCell.Entities)
                {
                    Entity updateUnitCell = new Entity("ssg_cell");
                    updateUnitCell.Attributes.Add("ssg_cellid", UnitCell.GetAttributeValue<Guid>("ssg_cellid"));
                    updateUnitCell.Attributes.Add("ssg_celllocationupdated", false);
                    _service.Update(updateUnitCell);
                    if (!UnitCell.Contains("ssg_clientid"))
                        GenerateNewDT(UnitCell, false, "Changed Unit Cell", DateTime.Now);
                }
            }

        }

        public void DeactivateDailyTracking()
        {

            foreach (var DT in lstCurrentDT)
            {
                _service.Update(DT);
            }
        }

        public void DeactivateDT(Entity client, Guid dailyTrackingId)
        {
            if (client.Contains("ssg_dailytrackingid"))
            {
                //complete exisiting DT
                Entity existingDT = new Entity("ssg_dailytracking");
                existingDT.Attributes.Add("ssg_dailytrackingid", dailyTrackingId);

                existingDT.Attributes.Add("statuscode", new OptionSetValue(2));
                existingDT.Attributes.Add("statecode", new OptionSetValue(1));
                lstCurrentDT.Add(existingDT);
            }
        }


        public void GenerateNewDT(Entity client, bool isLocationChange, String sMessage, DateTime dtDay)
        {
            //create new DT
            Entity enDT = new Entity("ssg_dailytracking");

            enDT.Attributes.Add("ssg_name", dtDay.ToShortDateString() + " " + dtDay.ToShortTimeString());
            enDT.Attributes.Add("ssg_date", dtDay);
            enDT.Attributes.Add("ssg_leavessequenceincrementby", 1);
            enDT.Attributes.Add("ssg_leavessequencenumber", 1);
            if (client.Contains("contactid"))
                enDT.Attributes.Add("ssg_client", new EntityReference("contact", client.GetAttributeValue<Guid>("contactid")));
            if (client.Contains("ssg_csnumber"))
                enDT.Attributes.Add("ssg_csnumber", client.GetAttributeValue<String>("ssg_csnumber"));

            if (client.Contains("ssg_correctionalcentre"))
                enDT.Attributes.Add("ssg_correctionalcentre", client.GetAttributeValue<EntityReference>("ssg_correctionalcentre"));
            else if (client.Contains("ssg_businessunit"))
                enDT.Attributes.Add("ssg_correctionalcentre", client.GetAttributeValue<EntityReference>("ssg_businessunit"));

            if (client.Contains("ssg_cellid") && client.LogicalName == "ssg_cell")
                enDT.Attributes.Add("ssg_cell", new EntityReference("ssg_cell", client.GetAttributeValue<Guid>("ssg_cellid")));
            else if (client.Contains("ssg_cellid"))
                enDT.Attributes.Add("ssg_cell", client.GetAttributeValue<EntityReference>("ssg_cellid"));

            if (client.Contains("unitcell.ssg_cellcode"))
                enDT.Attributes.Add("ssg_celllocation", client.GetAttributeValue<AliasedValue>("unitcell.ssg_cellcode").Value.ToString());
            else if (client.Contains("ssg_cellcode"))
                enDT.Attributes.Add("ssg_celllocation", client.GetAttributeValue<String>("ssg_cellcode"));
            else if (client.Contains("ssg_celllocation"))
                enDT.Attributes.Add("ssg_celllocation", client.GetAttributeValue<String>("ssg_celllocation"));

            if (client.Contains("unitcell.ssg_unitcode"))
                enDT.Attributes.Add("ssg_unitcell", client.GetAttributeValue<AliasedValue>("unitcell.ssg_unitcode").Value.ToString());
            else if (client.Contains("ssg_unitcode"))
                enDT.Attributes.Add("ssg_unitcell", client.GetAttributeValue<String>("ssg_unitcode"));

           
                       
            if (client.Contains("ssg_populationdesignation") && client.GetAttributeValue<String>("ssg_populationdesignation").ToString() != String.Empty)
            {
                trace.Trace("ssg_populationdesignation " + client.GetAttributeValue<String>("ssg_populationdesignation").ToString());
                if (client.GetAttributeValue<String>("ssg_populationdesignation").ToString().Substring(0, 2) == "GP")
                    enDT.Attributes.Add("ssg_popdesignation", "GP");
                if (client.GetAttributeValue<String>("ssg_populationdesignation").ToString().Substring(0, 2) == "PC")
                    enDT.Attributes.Add("ssg_popdesignation", "PC");
            }

            if (client.Contains("ssg_mentalhealthneeds"))
                enDT.Attributes.Add("ssg_mentalhealthneeds", client.GetAttributeValue<OptionSetValue>("ssg_mentalhealthneeds"));

            if (client.Contains("ssg_indigenous") && client.GetAttributeValue<Boolean>("ssg_indigenous") == true)
                enDT.Attributes.Add("ssg_characterindigenous", new OptionSetValue(867670000));
            else if (client.Contains("ssg_indigenous") && client.GetAttributeValue<Boolean>("ssg_indigenous") == false)
                enDT.Attributes.Add("ssg_characterindigenous", new OptionSetValue(867670001));                                                    

            if (client.Contains("ssg_dualbunkable"))
                enDT.Attributes.Add("ssg_dualbunkable", (client.GetAttributeValue<OptionSetValue>("ssg_dualbunkable")));

            if (client.Contains("DT.ssg_15minutechecks"))
                enDT.Attributes.Add("ssg_15minutechecks", (OptionSetValue)(client.GetAttributeValue<AliasedValue>("DT.ssg_15minutechecks").Value));
            else if (client.Contains("LDT.ssg_15minutechecks"))
                enDT.Attributes.Add("ssg_15minutechecks", (OptionSetValue)(client.GetAttributeValue<AliasedValue>("LDT.ssg_15minutechecks").Value));
            if (client.Contains("DT.ssg_specialhandlingprotocols"))
                enDT.Attributes.Add("ssg_specialhandlingprotocols", (OptionSetValue)(client.GetAttributeValue<AliasedValue>("DT.ssg_specialhandlingprotocols").Value));
            else if (client.Contains("LDT.ssg_specialhandlingprotocols"))
                enDT.Attributes.Add("ssg_specialhandlingprotocols", (OptionSetValue)(client.GetAttributeValue<AliasedValue>("LDT.ssg_specialhandlingprotocols").Value));
            if (client.Contains("DT.ssg_cgicrating"))
                enDT.Attributes.Add("ssg_cgicrating", (OptionSetValue)(client.GetAttributeValue<AliasedValue>("DT.ssg_cgicrating").Value));
            else if (client.Contains("LDT.ssg_cgicrating"))
                enDT.Attributes.Add("ssg_cgicrating", (OptionSetValue)(client.GetAttributeValue<AliasedValue>("LDT.ssg_cgicrating").Value));
            if (client.Contains("DT.ssg_completedcaseplan"))
                enDT.Attributes.Add("ssg_completedcaseplan", (OptionSetValue)(client.GetAttributeValue<AliasedValue>("DT.ssg_completedcaseplan").Value));
            else if (client.Contains("LDT.ssg_completedcaseplan"))
                enDT.Attributes.Add("ssg_completedcaseplan", (OptionSetValue)(client.GetAttributeValue<AliasedValue>("LDT.ssg_completedcaseplan").Value));
            if (client.Contains("DT.ssg_securitycautionsalertsnotes"))
                enDT.Attributes.Add("ssg_securitycautionsalertsnotes", client.GetAttributeValue<AliasedValue>("DT.ssg_securitycautionsalertsnotes").Value.ToString());
            else if (client.Contains("LDT.ssg_securitycautionsalertsnotes"))
                enDT.Attributes.Add("ssg_securitycautionsalertsnotes", client.GetAttributeValue<AliasedValue>("LDT.ssg_securitycautionsalertsnotes").Value.ToString());
 
            // cloning the daily tracking record created on the same day to copy over leaves/visits and client logs
            if (client.Contains("ssg_dailytrackingid") && client.Contains("DT.ssg_date") && ((DateTime)(client.GetAttributeValue<AliasedValue>("DT.ssg_date").Value)).Date == DateTime.Today.Date)
            {
                enDT.Attributes.Add("ssg_clonefromdailytrackingid", client.GetAttributeValue<EntityReference>("ssg_dailytrackingid"));
                _sPrevDTFilter = _sPrevDTFilter + "<value>" + client.GetAttributeValue<EntityReference>("ssg_dailytrackingid").Id.ToString() + "</value>";
            }  
            else if (client.Contains("ssg_lastdailytrackingid") && client.Contains("LDT.ssg_date") && ((DateTime)(client.GetAttributeValue<AliasedValue>("LDT.ssg_date").Value)).Date == DateTime.Today.Date)
            {
                enDT.Attributes.Add("ssg_clonefromdailytrackingid", client.GetAttributeValue<EntityReference>("ssg_lastdailytrackingid"));
                //_sPrevDTFilter = _sPrevDTFilter + "<value>" + client.GetAttributeValue<EntityReference>("ssg_lastdailytrackingid").Id.ToString() + "</value>";
            }

            if (client.Contains("DT.ssg_healthcareconsultcompleted"))
                enDT.Attributes.Add("ssg_healthcareconsultcompleted", (OptionSetValue)(client.GetAttributeValue<AliasedValue>("DT.ssg_healthcareconsultcompleted").Value));
            else if (client.Contains("LDT.ssg_healthcareconsultcompleted"))
                enDT.Attributes.Add("ssg_healthcareconsultcompleted", (OptionSetValue)(client.GetAttributeValue<AliasedValue>("LDT.ssg_healthcareconsultcompleted").Value));
            if (client.Contains("DT.ssg_managerreviewcompleted"))
                enDT.Attributes.Add("ssg_managerreviewcompleted", (OptionSetValue)(client.GetAttributeValue<AliasedValue>("DT.ssg_managerreviewcompleted").Value));
            else if (client.Contains("LDT.ssg_managerreviewcompleted"))
                enDT.Attributes.Add("ssg_managerreviewcompleted", (OptionSetValue)(client.GetAttributeValue<AliasedValue>("LDT.ssg_managerreviewcompleted").Value));
            if (client.Contains("DT.ssg_healthcareconsulttime"))
                enDT.Attributes.Add("ssg_healthcareconsulttime", (DateTime)(client.GetAttributeValue<AliasedValue>("DT.ssg_healthcareconsulttime").Value));
            else if (client.Contains("LDT.ssg_healthcareconsulttime"))
                enDT.Attributes.Add("ssg_healthcareconsulttime", (DateTime)(client.GetAttributeValue<AliasedValue>("LDT.ssg_healthcareconsulttime").Value));
            if (client.Contains("DT.ssg_managerreviewtime"))
                enDT.Attributes.Add("ssg_managerreviewtime", (DateTime)(client.GetAttributeValue<AliasedValue>("DT.ssg_managerreviewtime").Value));
            else if (client.Contains("LDT.ssg_managerreviewtime"))
                enDT.Attributes.Add("ssg_managerreviewtime", (DateTime)(client.GetAttributeValue<AliasedValue>("LDT.ssg_managerreviewtime").Value));
            
            if (client.Contains("ssg_lastcontinuousperiodofseparateconfinement") && client.Contains("CTOC.statecode") && ((OptionSetValue)(client.GetAttributeValue<AliasedValue>("CTOC.statecode").Value)).Value == 0)
                enDT.Attributes.Add("ssg_continuousseparateconfinementid", client.GetAttributeValue<EntityReference>("ssg_lastcontinuousperiodofseparateconfinement"));

            if (sMessage == "BackDated")
            {
                enDT.Attributes.Add("statuscode", new OptionSetValue(867670001));
                enDT.Attributes.Add("statecode", new OptionSetValue(0));
                enDT.Attributes.Add("ssg_lastactive", true);
                lstNewDT.Add(enDT);
            }
            else
            {
                enDT.Attributes.Add("statuscode", new OptionSetValue(1));
                enDT.Attributes.Add("statecode", new OptionSetValue(0));
                lstNewDT.Add(enDT);
            }
        }

        public void GenerateRecords()
        {
            
            foreach (var DT in lstNewDT)
            {
                _service.Create(DT);

            }
            foreach (var client in lstUpdatedClient)
            {
                _service.Update(client);
            }
        }

        public void ClearClientFlag(Entity client,String action)
        {

            //client["ssg_clientcellchanged"] = new OptionSetValue(867670001);
            Entity contact = new Entity("contact");
            contact.Attributes.Add("contactid", client["contactid"]);
            contact.Attributes.Add("ssg_deactivatedt", new OptionSetValue(867670001));
            contact.Attributes.Add("ssg_regeneratedt", new OptionSetValue(867670001));
            contact.Attributes.Add("ssg_clonedt", new OptionSetValue(867670001));
            contact.Attributes.Add("ssg_backdateddt", new OptionSetValue(867670001));
            if (action == "Deactivate")
                contact.Attributes.Add("ssg_dailytrackingid", null);
            lstUpdatedClient.Add(contact);


        }

        public void CreateRosterRefreshLog(String status, String details)
        {

            Entity enRRL = new Entity("ssg_rosterrefreshlog");
            enRRL.Attributes.Add("ssg_processcompletiontime", DateTime.Now);
            enRRL.Attributes.Add("ssg_processstatus", status);
            enRRL.Attributes.Add("ssg_processdetails", details);
            //enRRL.Attributes.Add("statuscode", new OptionSetValue(2));
            //enRRL.Attributes.Add("statecode", new OptionSetValue(1));

            _service.Create(enRRL);

        }

        public void BackDatedDT(Entity enClient)
        {
            var fetchBackDatedDT = "<fetch distinct='false' mapping='logical' output-format='xml-platform' version='1.0'>" +
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
                                    "<order descending='false' attribute='ssg_date'/>" +
                                    "<filter type = 'and'>" +
                                    "<condition attribute = 'ssg_date' operator= 'on-or-after'  value='" + ((DateTime)enClient.GetAttributeValue<AliasedValue>("CTOC.ssg_confinementstartdate").Value).ToShortDateString() + "'/>" +
                                    "<condition attribute = 'ssg_date' operator= 'on-or-before'  value='" + DateTime.Today.AddSeconds(-1).ToString() + "'/>" +
                                    "<condition attribute = 'ssg_client' operator= 'eq' value='"+enClient.GetAttributeValue<Guid>("contactid").ToString()+ "'/>" +
                                    "</filter >" +
                                    "</entity>" +
                                    "</fetch>";
            EntityCollection ecBackDatedDT = _service.RetrieveMultiple(new FetchExpression(fetchBackDatedDT));
            List<DateTime> lstDTDates = new List<DateTime>();

            if(ecBackDatedDT.Entities.Count >0)
            {
                foreach(var dt in ecBackDatedDT.Entities)
                {
                    if(!lstDTDates.Contains(dt.GetAttributeValue<DateTime>("ssg_date").ToLocalTime().Date))
                    {
                        lstDTDates.Add(dt.GetAttributeValue<DateTime>("ssg_date").ToLocalTime().Date);
                    }
                }
            }
            
            for (var day = DateTime.SpecifyKind(((DateTime)enClient.GetAttributeValue<AliasedValue>("CTOC.ssg_confinementstartdate").Value).Date, DateTimeKind.Local); day.Date < DateTime.SpecifyKind(DateTime.Today.Date,DateTimeKind.Local); day = day.AddDays(1))
            {
                if(!lstDTDates.Contains(day))
                {
                    GenerateNewDT(enClient, false,"BackDated",day);
                }

            }
            
        }
    }
}
