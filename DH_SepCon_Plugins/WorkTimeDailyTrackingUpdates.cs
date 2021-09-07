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
    public class WorkTimeDailyTrackingUpdates : IPlugin
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
            _context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));



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
                                        "<attribute name='ssg_indigenous'/>" +
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
                                        "<attribute name='ssg_populationdesignation'/>"+
                                        "<attribute name='ssg_dualbunkable'/>" +
                                        "<attribute name='ssg_csnumber'/>" +
                                        "<order attribute='fullname' descending='false'/>" +
                                        "<filter type='and'>" +
                                        "<filter type='or'>" +
                                        "<condition attribute='ssg_clientcellchanged' operator='eq' value='867670000'/>" +
                                        "<condition attribute='ssg_clientinconfinement' operator='eq' value='867670000'/>" +
                                        "<condition attribute='ssg_clientoutofconfinement' operator='eq' value='867670000'/>" +
                                        "<condition attribute='ssg_clientlocationchanged' operator='eq' value='867670000'/>" +
                                        "</filter>" +
                                        "</filter>" +
                                        "<link-entity name='ssg_separateconfinementperiod' from='ssg_separateconfinementperiodid' to='ssg_lastcontinuousperiodofseparateconfinement' visible='false' link-type='outer' alias='CTOC'>" +
                                        "<attribute name='statecode'/>" +
                                        "</link-entity>" +
                                        "<link-entity name='ssg_dailytracking' from='ssg_dailytrackingid' to='ssg_dailytrackingid' visible='false' link-type='outer' alias='DT'>" +
                                        "<attribute name='ssg_unitcell'/>" +
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
                    #region Client Cell Changed= true


                    //If Client Unit = null & DT's Unit has data & CTOC's status = Inactive, then complete the Daily Tracking
                    List<Entity> lstClientCellCleared = ecModifiedClients.Entities.Where(p => !p.Contains("ssg_cellid") && !p.Contains("ssg_celllocation")
                                                            && p.Contains("DT.ssg_cell")
                                                            && (!p.Contains("CTOC.statecode") || ((OptionSetValue)(p.GetAttributeValue<AliasedValue>("CTOC.statecode").Value)).Value == 1)
                                                            && p.Contains("ssg_clientcellchanged")
                                                            && p.GetAttributeValue<OptionSetValue>("ssg_clientcellchanged").Value == 867670000).Select(p => p).ToList();

                    if (lstClientCellCleared.Count > 0)
                    {
                        trace.Trace("Client Unit = null & DT's Unit has data & CTOC's status = Inactive: " + lstClientCellCleared.Count.ToString());
                        foreach (var client in lstClientCellCleared)
                        {
                            if (client.Contains("ssg_dailytrackingid"))
                                DeactivateDT(client, client.GetAttributeValue<EntityReference>("ssg_dailytrackingid").Id);
                            ClearClientFlag(client);
                            client.Attributes.Add("Considered", true);
                        }

                    }

                    //If Client Unit = null & DT's Unit has data & CTOC's status = Active, then complete the Daily Tracking and generate new DT
                    List<Entity> lstClientCellClearedWithCTOC = ecModifiedClients.Entities.Where(p => !p.Contains("ssg_cellid") && !p.Contains("ssg_celllocation")
                                                            && p.Contains("DT.ssg_cell")
                                                            && (p.Contains("CTOC.statecode") && ((OptionSetValue)(p.GetAttributeValue<AliasedValue>("CTOC.statecode").Value)).Value == 0)
                                                            && p.Contains("ssg_clientcellchanged")
                                                            && p.GetAttributeValue<OptionSetValue>("ssg_clientcellchanged").Value == 867670000).Select(p => p).ToList();
                    if (lstClientCellClearedWithCTOC.Count > 0)
                    {
                        trace.Trace("Client Unit = null & DT's Unit has data & CTOC's status = Active: " + lstClientCellClearedWithCTOC.Count.ToString());
                        foreach (var client in lstClientCellClearedWithCTOC)
                        {
                            if (client.Contains("ssg_dailytrackingid"))
                                DeactivateDT(client, client.GetAttributeValue<EntityReference>("ssg_dailytrackingid").Id);

                            GenerateNewDT(client,false);
                            ClearClientFlag(client);
                            client.Attributes.Add("Considered", true);
                        }

                    }

                    //If Client Unit has data & DT's Unit = null (don't have to worry about CTOC), then complete the Daily Tracking and generate new DT
                    List<Entity> lstClientCellNew = ecModifiedClients.Entities.Where(p => p.Contains("ssg_cellid")
                                                            && !p.Contains("DT.ssg_cell")
                                                            && p.Contains("ssg_clientcellchanged")
                                                            && p.GetAttributeValue<OptionSetValue>("ssg_clientcellchanged").Value == 867670000).Select(p => p).ToList();

                    if (lstClientCellNew.Count > 0)
                    {
                        trace.Trace("Client Unit has data & DT's Unit = null (don't have to worry about CTOC): " + lstClientCellNew.Count.ToString());
                        foreach (var client in lstClientCellNew)
                        {
                            if (client.Contains("ssg_dailytrackingid"))
                                DeactivateDT(client, client.GetAttributeValue<EntityReference>("ssg_dailytrackingid").Id);

                            GenerateNewDT(client,false);
                            ClearClientFlag(client);
                            client.Attributes.Add("Considered", true);
                        }

                    }

                    //If Client Unit = null & DT's Unit = null and CTOC's Status = Active, then complete the Daily Tracking and generate new DT
                    List<Entity> lstClientInConfinement = ecModifiedClients.Entities.Where(p => !p.Contains("ssg_cellid") && !p.Contains("ssg_celllocation")
                                                            && !p.Contains("DT.ssg_cell")
                                                            && (p.Contains("CTOC.statecode") && ((OptionSetValue)(p.GetAttributeValue<AliasedValue>("CTOC.statecode").Value)).Value == 0)
                                                            && p.Contains("ssg_clientcellchanged")
                                                            && p.GetAttributeValue<OptionSetValue>("ssg_clientcellchanged").Value == 867670000).Select(p => p).ToList();

                    if (lstClientInConfinement.Count > 0)
                    {
                        trace.Trace("Client Unit = null & DT's Unit = null and CTOC's Status = Active: " +lstClientInConfinement.Count.ToString());
                        foreach (var client in lstClientInConfinement)
                        {
                            if(client.Contains("ssg_dailytrackingid"))
                                DeactivateDT(client, client.GetAttributeValue<EntityReference>("ssg_dailytrackingid").Id);

                            GenerateNewDT(client,false);
                            ClearClientFlag(client);
                            client.Attributes.Add("Considered", true);
                        }

                    }


                    //If Client Unit = null & DT's Unit = null and CTOC's Status = Inactive, then complete the Daily Tracking and generate new DT
                    List<Entity> lstClientOutOfConfinementNoUnit = ecModifiedClients.Entities.Where(p => !p.Contains("ssg_cellid") && !p.Contains("ssg_celllocation")
                                                            && !p.Contains("DT.ssg_cell")
                                                            && ((p.Contains("CTOC.statecode") && ((OptionSetValue)(p.GetAttributeValue<AliasedValue>("CTOC.statecode").Value)).Value == 1) || !p.Contains("CTOC.statecode"))
                                                            && p.Contains("ssg_clientcellchanged")
                                                            && p.GetAttributeValue<OptionSetValue>("ssg_clientcellchanged").Value == 867670000).Select(p => p).ToList();

                    if (lstClientOutOfConfinementNoUnit.Count > 0)
                    {
                        trace.Trace("Client Unit = null & DT's Unit = null and CTOC's Status = Active: " + lstClientInConfinement.Count.ToString());
                        foreach (var client in lstClientOutOfConfinementNoUnit)
                        {
                            if (client.Contains("ssg_dailytrackingid"))
                                DeactivateDT(client, client.GetAttributeValue<EntityReference>("ssg_dailytrackingid").Id);

                            ClearClientFlag(client);
                            client.Attributes.Add("Considered", true);
                        }

                    }


                    //If Client Unit != DT's Unit (don't have to worry about CTOC), then complete the Daily Tracking and generate new DT
                    List<Entity> lstClientCellChanged = ecModifiedClients.Entities.Where(p => p.Contains("ssg_cellid") && p.Contains("DT.ssg_cell")
                                                             && p.GetAttributeValue<EntityReference>("ssg_cellid").Id != ((EntityReference)(p.GetAttributeValue<AliasedValue>("DT.ssg_cell").Value)).Id
                                                             && p.Contains("ssg_clientcellchanged")
                                                             && p.GetAttributeValue<OptionSetValue>("ssg_clientcellchanged").Value == 867670000).Select(p => p).ToList();

                    if (lstClientCellChanged.Count > 0)
                    {
                        trace.Trace("Client Unit != DT's Unit (don't have to worry about CTOC)" + lstClientCellChanged.Count.ToString());
                        foreach (var client in lstClientCellChanged)
                        {
                            if (client.Contains("ssg_dailytrackingid"))
                                DeactivateDT(client, client.GetAttributeValue<EntityReference>("ssg_dailytrackingid").Id);

                            GenerateNewDT(client,false);
                            ClearClientFlag(client);
                            client.Attributes.Add("Considered", true);
                        }

                    }


                    //If Client Unit = null, Client Cell location has value and CTOC is active, then complete the Daily Tracking and generate new DT
                    List<Entity> lstClientCellChangedOutsideSegConf = ecModifiedClients.Entities.Where(p => !p.Contains("ssg_cellid")
                                                            && p.Contains("ssg_celllocation")
                                                            && (p.Contains("CTOC.statecode") && ((OptionSetValue)(p.GetAttributeValue<AliasedValue>("CTOC.statecode").Value)).Value == 0)

                                                            && p.Contains("ssg_clientcellchanged")
                                                             && p.GetAttributeValue<OptionSetValue>("ssg_clientcellchanged").Value == 867670000).Select(p => p).ToList();

                    if (lstClientCellChangedOutsideSegConf.Count > 0)
                    {
                        trace.Trace("Client Unit = null, Client Cell location has value, DT's Unit contains data " + lstClientCellChanged.Count.ToString());
                        foreach (var client in lstClientCellChangedOutsideSegConf)
                        {
                            if (client.Contains("ssg_dailytrackingid"))
                                DeactivateDT(client, client.GetAttributeValue<EntityReference>("ssg_dailytrackingid").Id);

                            GenerateNewDT(client,false);
                            ClearClientFlag(client);
                            client.Attributes.Add("Considered", true);
                        }

                    }

                    //If Client Unit = null, Client Cell location has value and CTOC is Inactive, then complete the Daily Tracking and generate new DT
                    List<Entity> lstClientCellChangedOutsideSegConfNoCTOC = ecModifiedClients.Entities.Where(p => !p.Contains("ssg_cellid")
                                                            && p.Contains("ssg_celllocation")
                                                            && ((!p.Contains("CTOC.statecode") || (p.Contains("CTOC.statecode") && ((OptionSetValue)(p.GetAttributeValue<AliasedValue>("CTOC.statecode").Value)).Value == 1)))

                                                            && p.Contains("ssg_clientcellchanged")
                                                             && p.GetAttributeValue<OptionSetValue>("ssg_clientcellchanged").Value == 867670000).Select(p => p).ToList();

                    if (lstClientCellChangedOutsideSegConfNoCTOC.Count > 0)
                    {
                        trace.Trace("Client Unit = null, Client Cell location has value and CTOC is Inactive " + lstClientCellChangedOutsideSegConfNoCTOC.Count.ToString());
                        foreach (var client in lstClientCellChangedOutsideSegConfNoCTOC)
                        {
                            if (client.Contains("ssg_dailytrackingid"))
                                DeactivateDT(client, client.GetAttributeValue<EntityReference>("ssg_dailytrackingid").Id);

                            
                            ClearClientFlag(client);
                            client.Attributes.Add("Considered", true);
                        }

                    }

                    #endregion

                    #region Client Cell Changed = false and Client in Confinement= true

                    //If Client has a daily tracking record , then link the DT with CTOC
                    List<Entity> lstClientConfinement = ecModifiedClients.Entities.Where(p => p.Contains("ssg_dailytrackingid") && p.Contains("ssg_lastcontinuousperiodofseparateconfinement")
                                                             && (!p.Contains("ssg_clientcellchanged") || p.GetAttributeValue<OptionSetValue>("ssg_clientcellchanged").Value == 867670001)
                                                             && p.GetAttributeValue<OptionSetValue>("ssg_clientinconfinement").Value == 867670000).Select(p => p).ToList();
                    if (lstClientConfinement.Count > 0)
                    {
                        trace.Trace("Client Cell Changed = false and Client in Confinement= true: Client has a daily tracking record: " + lstClientConfinement.Count.ToString());
                        foreach (var client in lstClientConfinement)
                        {
                            //Link DT to CTOC
                            Entity existingDT = new Entity("ssg_dailytracking");
                            existingDT.Attributes.Add("ssg_dailytrackingid", client.GetAttributeValue<EntityReference>("ssg_dailytrackingid").Id);
                            existingDT.Attributes.Add("ssg_continuousseparateconfinementid", new EntityReference("ssg_separateconfinementperiod", client.GetAttributeValue<EntityReference>("ssg_lastcontinuousperiodofseparateconfinement").Id));

                            lstCurrentDT.Add(existingDT);
                            ClearClientFlag(client);
                            client.Attributes.Add("Considered", true);

                        }

                    }

                    //If Client doesn't have a daily tracking , then link the DT with CTOC
                    List<Entity> lstClientNewConfinement = ecModifiedClients.Entities.Where(p => !p.Contains("ssg_dailytrackingid")
                                                             &&
                                                            (!p.Contains("ssg_clientcellchanged") || p.GetAttributeValue<OptionSetValue>("ssg_clientcellchanged").Value == 867670001)
                                                             &&
                                                             p.GetAttributeValue<OptionSetValue>("ssg_clientinconfinement").Value == 867670000
                                                             ).Select(p => p).ToList();

                    if (lstClientNewConfinement.Count > 0)
                    {
                        trace.Trace("Client Cell Changed = false and Client in Confinement= true: Client doesn't have a daily tracking: " + lstClientNewConfinement.Count.ToString());
                        foreach (var client in lstClientNewConfinement)
                        {
                            GenerateNewDT(client,false);

                            ClearClientFlag(client);
                            client.Attributes.Add("Considered", true);

                        }

                    }

                    #endregion

                    #region Client Cell Changed = false and Client in Confinement= false and Client out of Confinement = true

                    //If Client Unit  doesn't  contains data and Cell location doesn't contain data, then deactivate the DT with CTOC
                    List<Entity> lstClientOutOfConfinement = ecModifiedClients.Entities.Where(p => !p.Contains("DT.ssg_cellid")
                                                             && !p.Contains("ssg_celllocation")
                                                             && p.Contains("ssg_dailytrackingid")
                                                             && (!p.Contains("ssg_clientcellchanged") || p.GetAttributeValue<OptionSetValue>("ssg_clientcellchanged").Value == 867670001)
                                                             && (!p.Contains("ssg_clientinconfinement") || p.GetAttributeValue<OptionSetValue>("ssg_clientinconfinement").Value == 867670001)
                                                             && p.GetAttributeValue<OptionSetValue>("ssg_clientoutofconfinement").Value == 867670000).Select(p => p).ToList();
                    if (lstClientOutOfConfinement.Count > 0)
                    {
                        trace.Trace("Client Cell Changed = false and Client in Confinement= false and Client out of Confinement = true: Client Unit doesn't contain data: " + lstClientOutOfConfinement.Count.ToString());
                        foreach (var client in lstClientOutOfConfinement)
                        {
                            if (client.Contains("ssg_dailytrackingid"))
                                DeactivateDT(client, client.GetAttributeValue<EntityReference>("ssg_dailytrackingid").Id);
                            ClearClientFlag(client);
                            client.Attributes.Add("Considered", true);
                        }
                    }

                    //If Client Unit contains data , then deassociate CTOC and Client the DT with CTOC
                    List<Entity> lstClientOutOfConfinementWithUnit = ecModifiedClients.Entities.Where(p => p.Contains("ssg_celllocation") && p.Contains("ssg_cellid")
                                                                
                                                             && p.Contains("ssg_dailytrackingid")
                                                             && (!p.Contains("ssg_clientcellchanged") || p.GetAttributeValue<OptionSetValue>("ssg_clientcellchanged").Value == 867670001)
                                                             && (!p.Contains("ssg_clientinconfinement") || p.GetAttributeValue<OptionSetValue>("ssg_clientinconfinement").Value == 867670001)
                                                             && p.GetAttributeValue<OptionSetValue>("ssg_clientoutofconfinement").Value == 867670000).Select(p => p).ToList();
                    if (lstClientOutOfConfinementWithUnit.Count > 0)
                    {
                        trace.Trace("Client Cell Changed = false and Client in Confinement= false and Client out of Confinement = true: Client Unit contains data: " + lstClientOutOfConfinementWithUnit.Count.ToString());
                        foreach (var client in lstClientOutOfConfinementWithUnit)
                        {
                            if (client.Contains("ssg_dailytrackingid"))
                                DeactivateDT(client, client.GetAttributeValue<EntityReference>("ssg_dailytrackingid").Id);

                            GenerateNewDT(client, false);

                            ClearClientFlag(client);
                            client.Attributes.Add("Considered", true);
                        }
                    }

                    //If Client cell location contains data and unit/cell is null and CTOC is Inactive, then deassociate CTOC and Client the DT with CTOC
                    List<Entity> lstClientOutOfConfinementWithUnitWDT = ecModifiedClients.Entities.Where(p => p.Contains("ssg_celllocation") && !p.Contains("ssg_cellid")
                                                              && (p.Contains("CTOC.statecode") && ((OptionSetValue)(p.GetAttributeValue<AliasedValue>("CTOC.statecode").Value)).Value == 1)
                                                             
                                                             && !p.Contains("Considered")
                                                             && p.GetAttributeValue<OptionSetValue>("ssg_clientoutofconfinement").Value == 867670000).Select(p => p).ToList();
                    if (lstClientOutOfConfinementWithUnitWDT.Count > 0)
                    {
                        trace.Trace("Client Cell Changed = false and Client in Confinement= false and Client out of Confinement = true: Client Unit contains data: " + lstClientOutOfConfinementWithUnit.Count.ToString());
                        foreach (var client in lstClientOutOfConfinementWithUnitWDT)
                        {
                            if (client.Contains("ssg_dailytrackingid"))
                                DeactivateDT(client, client.GetAttributeValue<EntityReference>("ssg_dailytrackingid").Id);

                            

                            ClearClientFlag(client);
                            client.Attributes.Add("Considered", true);
                        }
                    }


                    //If Client cell location contains data and unit/cell is null and CTOC is Active, then deassociate CTOC and Client the DT with CTOC
                    List<Entity> lstClientOutOfConfinementWithUnitWODT = ecModifiedClients.Entities.Where(p => p.Contains("ssg_celllocation") && !p.Contains("ssg_cellid")
                                                              && (p.Contains("CTOC.statecode") && ((OptionSetValue)(p.GetAttributeValue<AliasedValue>("CTOC.statecode").Value)).Value == 0)
                                                              && !p.Contains("ssg_dailytrackingid")
                                                             && (!p.Contains("ssg_clientcellchanged") || p.GetAttributeValue<OptionSetValue>("ssg_clientcellchanged").Value == 867670001)
                                                             && (!p.Contains("ssg_clientinconfinement") || p.GetAttributeValue<OptionSetValue>("ssg_clientinconfinement").Value == 867670001)
                                                             && p.GetAttributeValue<OptionSetValue>("ssg_clientoutofconfinement").Value == 867670000).Select(p => p).ToList();
                    if (lstClientOutOfConfinementWithUnitWODT.Count > 0)
                    {
                        trace.Trace("Client Cell Changed = false and Client in Confinement= false and Client out of Confinement = true: Client Unit contains data: " + lstClientOutOfConfinementWithUnit.Count.ToString());
                        foreach (var client in lstClientOutOfConfinementWithUnitWODT)
                        {
                            

                            GenerateNewDT(client,false);

                            ClearClientFlag(client);
                            client.Attributes.Add("Considered", true);
                        }
                    }

                    #endregion

                    #region
                    List<Entity> lstClientOutOfLocation = ecModifiedClients.Entities.Where(p => p.Contains("ssg_dailytrackingid")
                                                             && (!p.Contains("ssg_clientcellchanged") || p.GetAttributeValue<OptionSetValue>("ssg_clientcellchanged").Value == 867670001)
                                                             && (!p.Contains("ssg_clientinconfinement") || p.GetAttributeValue<OptionSetValue>("ssg_clientinconfinement").Value == 867670001)
                                                             && (!p.Contains("ssg_clientinconfinement") || p.GetAttributeValue<OptionSetValue>("ssg_clientoutofconfinement").Value == 867670001)
                                                             && p.GetAttributeValue<OptionSetValue>("ssg_clientlocationchanged").Value == 867670000).Select(p => p).ToList();
                    if (lstClientOutOfLocation.Count > 0)
                    {
                        trace.Trace("Client Cell Changed = false and Client in Confinement= false and Client out of Confinement = false and Client Location Changed = True:  " + lstClientOutOfLocation.Count.ToString());
                        foreach (var client in lstClientOutOfLocation)
                        {
                            if (client.Contains("ssg_dailytrackingid"))
                                DeactivateDT(client, client.GetAttributeValue<EntityReference>("ssg_dailytrackingid").Id);
                            ClearClientFlag(client);
                            client.Attributes.Add("Considered", true);

                            GenerateNewDT(client, true);
                        }
                    }
                        #endregion

                    }
                else
                {
                    UpdateRosterRefreshLog("Completed", "No Client found for Refresh");
                }

                DeactivateDailyTracking();
                FetchChangedUnitCell();
                GenerateRecords();

                UpdateRosterRefreshLog("Completed", "");
            }
            catch (Exception e)
            {

                UpdateRosterRefreshLog("Failed", e.Message);

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
                        GenerateNewDT(UnitCell,false);
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

        public void DeactivateDT(Entity client,Guid dailyTrackingId)
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


        public void GenerateNewDT(Entity client, bool isLocationChange)
        {
            //create new DT
            Entity enDT = new Entity("ssg_dailytracking");
            enDT.Attributes.Add("ssg_name", DateTime.Today.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());
            enDT.Attributes.Add("ssg_leavessequenceincrementby", 1);
            enDT.Attributes.Add("ssg_leavessequencenumber", 1);
            if (client.Contains("contactid"))
                enDT.Attributes.Add("ssg_client", new EntityReference("contact", client.GetAttributeValue<Guid>("contactid")));
            if (client.Contains("ssg_csnumber"))
                enDT.Attributes.Add("ssg_csnumber", client.GetAttributeValue<String>("ssg_csnumber"));

            if (client.Contains("ssg_correctionalcentre"))
                enDT.Attributes.Add("ssg_correctionalcentre", client.GetAttributeValue<EntityReference>("ssg_correctionalcentre"));
            else if(client.Contains("ssg_businessunit"))
                enDT.Attributes.Add("ssg_correctionalcentre", client.GetAttributeValue<EntityReference>("ssg_businessunit"));

            if(client.Contains("ssg_cellid") && client.LogicalName == "ssg_cell")
                enDT.Attributes.Add("ssg_cell", new EntityReference("ssg_cell", client.GetAttributeValue<Guid>("ssg_cellid")));
            else if(client.Contains("ssg_cellid"))
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

            enDT.Attributes.Add("ssg_date", DateTime.Today);

            
            
            if (client.Contains("ssg_populationdesignation") && client.GetAttributeValue<String>("ssg_populationdesignation").ToString() != String.Empty)
            {
                trace.Trace("ssg_populationdesignation " + client.GetAttributeValue<String>("ssg_populationdesignation").ToString());
                if (client.GetAttributeValue<String>("ssg_populationdesignation").ToString().Substring(0, 2) == "GP")
                    enDT.Attributes.Add("ssg_popdesignation", "GP");
                if (client.GetAttributeValue<String>("ssg_populationdesignation").ToString().Substring(0, 2) == "PC")
                    enDT.Attributes.Add("ssg_popdesignation", "PC");
            }
            

            
                if (client.Contains("DT.ssg_mentalhealthneeds"))
                    enDT.Attributes.Add("ssg_mentalhealthneeds", (bool)(client.GetAttributeValue<AliasedValue>("DT.ssg_mentalhealthneeds").Value));
                
                if (client.Contains("DT.ssg_characterindigenous"))
                    enDT.Attributes.Add("ssg_characterindigenous", (bool)(client.GetAttributeValue<AliasedValue>("DT.ssg_characterindigenous").Value));
                
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

            if (!isLocationChange)
            {

                if (client.Contains("ssg_dailytrackingid"))
                {
                    enDT.Attributes.Add("ssg_clonefromdailytrackingid", client.GetAttributeValue<EntityReference>("ssg_dailytrackingid"));
                    _sPrevDTFilter = _sPrevDTFilter + "<value>" + client.GetAttributeValue<EntityReference>("ssg_dailytrackingid").Id.ToString() + "</value>";
                }
                else if (client.Contains("ssg_lastdailytrackingid"))
                {
                    enDT.Attributes.Add("ssg_clonefromdailytrackingid", client.GetAttributeValue<EntityReference>("ssg_lastdailytrackingid"));

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
            }
            if (client.Contains("ssg_lastcontinuousperiodofseparateconfinement") &&  client.Contains("CTOC.statecode") && ((OptionSetValue)(client.GetAttributeValue<AliasedValue>("CTOC.statecode").Value)).Value == 0)
                enDT.Attributes.Add("ssg_continuousseparateconfinementid", client.GetAttributeValue<EntityReference>("ssg_lastcontinuousperiodofseparateconfinement"));


            lstNewDT.Add(enDT);
        }

        public void GenerateRecords()
        {

            foreach(var DT in lstNewDT)
            {
                _service.Create(DT);

            }
            foreach (var client in lstUpdatedClient)
            {
                _service.Update(client);
            }
        }

        public void ClearClientFlag(Entity client) 
        {
           
                //client["ssg_clientcellchanged"] = new OptionSetValue(867670001);
                Entity contact = new Entity("contact");
                contact.Attributes.Add("contactid", client["contactid"]);
                contact.Attributes["ssg_clientcellchanged"] = new OptionSetValue(867670001);
                contact.Attributes.Add("ssg_clientinconfinement", new OptionSetValue(867670001));
                contact.Attributes.Add("ssg_clientoutofconfinement", new OptionSetValue(867670001));
                contact.Attributes.Add("ssg_clientlocationchanged", new OptionSetValue(867670001));
            lstUpdatedClient.Add(contact);


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

            if(ecRRL.Entities.Count>0)
            {
                Entity enRRL = new Entity("ssg_rosterrefreshlog");
                enRRL.Attributes.Add("ssg_rosterrefreshlogid",ecRRL.Entities[0].Id);
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
