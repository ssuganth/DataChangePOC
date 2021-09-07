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
    public class BackdatedDTRefresh
    {
        List<Entity> lstDT = new List<Entity>();
        List<String> lstUnitCell = new List<String>();
        String sCTOCFilter = String.Empty;
       

        IOrganizationService _service;
        IPluginExecutionContext _context;
        DateTime _dtConfinementStart = new DateTime();
        EntityReference _erClient = null;
        DateTime _dtToday = DateTime.Today;
        
        ITracingService trace;
        public void Execute(IServiceProvider serviceProvider)
        {
            _context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            IOrganizationServiceFactory serviceFactory =
                   (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            _service = serviceFactory.CreateOrganizationService(_context.UserId);
            trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            trace.Trace("Backdated DTDailyRefresh - Begin");

            if (_context.InputParameters.Contains("StartDate"))
                _dtConfinementStart = DateTime.Parse(_context.InputParameters["StartDate"].ToString());

            if(_context.InputParameters.Contains("Client"))
                 _erClient = (EntityReference)_context.InputParameters["Target"];


            try
            {
                if (_dtConfinementStart != new DateTime() && _dtConfinementStart < _dtToday && _erClient!=null)
                {
                    for (var day = _dtConfinementStart.Date; day.Date <= _dtToday.Date; day = day.AddDays(1))
                    {
                        //Create new Daily Tracking records
                        CreateNewDT();

                    }                                     

                }
            }
            catch (Exception e)
            {
                
                throw new InvalidPluginExecutionException(e + "Backdated DTRefresh Plugin error: " + e.Message);
            }
        }


        /// <summary>
        /// Create new Daily Tracking Records
        /// </summary>
        public void CreateNewDT()
        {
            try
            {


                List<Entity> lstDT = new List<Entity>();
                List<String> lstUnitCell = new List<String>();
                String sCTOCFilter = String.Empty;


                #region Fetch last active Daily Tracking Record
                ////Fetch last active Daily Tracking Records to carry over few attribute values to the new records
                //var fetchDailyTracking = "<fetch distinct='false' mapping='logical' output-format='xml-platform' version='1.0'>" +
                //                "<entity name='ssg_dailytracking'>" +
                //                "<attribute name='ssg_name'/>" +
                //                "<attribute name='createdon'/>" +
                //                "<attribute name='ssg_date'/>" +
                //                "<attribute name='ssg_csnumber'/>" +
                //                "<attribute name='ssg_client'/>" +
                //                "<attribute name='ssg_celllocation'/>" +
                //                "<attribute name='ssg_dailytrackingid'/>" +
                //                "<attribute name='ssg_15minutechecks'/>" +
                //                "<attribute name='ssg_specialhandlingprotocols'/>" +
                //                "<attribute name='ssg_cgicrating'/>" +
                //                "<attribute name='ssg_completedcaseplan'/>" +
                //                "<attribute name='ssg_securitycautionsalertsnotes'/>" +
                //                "<order descending='true' attribute='ssg_date'/>" +
                //                "<filter type = 'and'>" +
                //                "<condition attribute = 'ssg_date' operator= 'yesterday'/>" +
                //                "<condition attribute = 'ssg_lastactive' operator= 'eq' value = '1'/>" +
                //                "<condition attribute = 'ssg_client' operator= 'not-null'/>" +
                //                 "<condition attribute = 'ssg_correctionalcentre' value ='" + _sExecutionBU + "' operator= 'eq'/>" +
                //                "</filter >" +
                //                "</entity>" +
                //                "</fetch>";

                //EntityCollection ecDT = _service.RetrieveMultiple(new FetchExpression(fetchDailyTracking));

                #endregion



                #region Fetch Client with Unit Cell
                //Fetch all the unit cell associated with a Client 
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
                                     "<filter type='and'>" +                                     
                                     "<condition attribute= 'statecode' value='0' operator='eq'/>" +
                                    "</filter>" +
                                    "<link-entity name='contact' alias='client' link-type='inner' visible='false' to='ssg_clientid' from='contactid'>" +
                                    "<attribute name='ssg_csnumber'/>" +
                                    "<attribute name='ssg_correctionalcentre'/>" +
                                    "<attribute name='ssg_populationdesignation'/>" +
                                    "<attribute name='ssg_lastcontinuousperiodofseparateconfinement'/>" +
                                    "<attribute name='ssg_dualbunkable'/>" +
                                    "<attribute name='ssg_indigenous'/>" +
                                        "<attribute name='ssg_mentalhealthneeds'/>" +
                                    "<filter type='and'>" +
                                    "<condition attribute = 'contactid' value = '"+ _erClient.Id +"' operator= 'eq'/>"+
                                             "</filter>" +
                                    "</link-entity>" +
                                    "</entity>" +
                                    "</fetch>";

                EntityCollection ecUnitCellWithClient = _service.RetrieveMultiple(new FetchExpression(fetchUnitCellWithClient));
                if (ecUnitCellWithClient.Entities.Count > 0)
                {
                    trace.Trace("DTDailyRefresh - Count of Cell associated with Client - " + ecUnitCellWithClient.Entities.Count.ToString());

                    //loop through all the unit cells and generate DTR's
                    foreach (var ecClient in ecUnitCellWithClient.Entities)
                    {

                        var enDT = GenerateDT(null, ecClient);

                        lstDT.Add(enDT);

                        //Generating CTOC filter - all the clients for who a DTR generated should not be considered in CTOC only fetch
                        sCTOCFilter = sCTOCFilter + "<value>" + ecClient.GetAttributeValue<EntityReference>("ssg_clientid").Id.ToString() + "</value>";
                    }
                }
                else {
                    #region Fetch Active CTOC Record

                    String fetchActiveCTOC = String.Empty;
                    //Insert CTOC filter as required
                    if (sCTOCFilter != String.Empty && sCTOCFilter != "" && sCTOCFilter != null)
                    {

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
                                                      "<condition attribute = 'ssg_client' value = '" + _erClient.Id + "' operator= 'eq'/>" +
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
                    }
                   

                    EntityCollection ecActiveCTOC = _service.RetrieveMultiple(new FetchExpression(fetchActiveCTOC));

                    if (ecActiveCTOC.Entities.Count > 0)
                    {
                        trace.Trace("DTDailyRefresh - Count of CTOC - " + ecActiveCTOC.Entities.Count.ToString());
                        foreach (var ecClient in ecActiveCTOC.Entities)
                        {
                            var enDT = GenerateDT(null, ecClient);

                            lstDT.Add(enDT);

                        }
                    }

                    #endregion
                }

                #endregion










                #region Generate Daily Tracking


                foreach (var enDT in lstDT)
                {
                    _service.Create(enDT);
                   
                }


                #endregion

               

            }
            catch (Exception e)
            {
                throw new InvalidPluginExecutionException(e + "DTDailyRefresh Plugin error: " + e.Message);
            }
        }


        public Entity GenerateDT(EntityCollection previousDTCollection, Entity ecClient)
        {


            Entity enDT = new Entity("ssg_dailytracking");
            enDT.Attributes.Add("ssg_name", DateTime.Today.ToShortDateString() + " " + DateTime.Today.ToShortTimeString());
            enDT.Attributes.Add("ssg_date", DateTime.Today);
            enDT.Attributes.Add("ssg_leavessequenceincrementby", 1);
            enDT.Attributes.Add("ssg_leavessequencenumber", 1);


            if (ecClient.Contains("ssg_client"))
                enDT.Attributes.Add("ssg_client", ecClient.GetAttributeValue<EntityReference>("ssg_client"));
            else if (ecClient.Contains("ssg_clientid"))
                enDT.Attributes.Add("ssg_client", ecClient.GetAttributeValue<EntityReference>("ssg_clientid"));
            if (ecClient.Contains("ssg_unitcode"))
                enDT.Attributes.Add("ssg_unitcell", ecClient.GetAttributeValue<String>("ssg_unitcode"));
            if (ecClient.Contains("ssg_cellcode"))
                enDT.Attributes.Add("ssg_celllocation", ecClient.GetAttributeValue<String>("ssg_cellcode"));
            else if (ecClient.Contains("client.ssg_celllocation"))
                enDT.Attributes.Add("ssg_celllocation", ecClient.GetAttributeValue<AliasedValue>("client.ssg_celllocation").Value.ToString());

            if (ecClient.Contains("client.ssg_csnumber"))
                enDT.Attributes.Add("ssg_csnumber", ecClient.GetAttributeValue<AliasedValue>("client.ssg_csnumber").Value.ToString());
            else if (ecClient.Contains("ssg_csnumber"))
                enDT.Attributes.Add("ssg_csnumber", ecClient.GetAttributeValue<String>("ssg_csnumber"));

            if (ecClient.Contains("client.ssg_correctionalcentre"))
                enDT.Attributes.Add("ssg_correctionalcentre", (EntityReference)(ecClient.GetAttributeValue<AliasedValue>("client.ssg_correctionalcentre").Value));
            else if (ecClient.Contains("ssg_correctioncentre"))
                enDT.Attributes.Add("ssg_correctionalcentre", ecClient.GetAttributeValue<EntityReference>("ssg_correctioncentre"));
            else if (ecClient.Contains("ssg_businessunit"))
                enDT.Attributes.Add("ssg_correctionalcentre", ecClient.GetAttributeValue<EntityReference>("ssg_businessunit"));

            if (ecClient.Contains("ssg_cellid"))
                enDT.Attributes.Add("ssg_cell", new EntityReference("ssg_cell", ecClient.GetAttributeValue<Guid>("ssg_cellid")));

            if (ecClient.Contains("client.ssg_populationdesignation") && ecClient.GetAttributeValue<AliasedValue>("client.ssg_populationdesignation").Value.ToString() != String.Empty)
            {
                if (ecClient.GetAttributeValue<AliasedValue>("client.ssg_populationdesignation").Value.ToString().Substring(0, 2) == "GP")
                    enDT.Attributes.Add("ssg_popdesignation", "GP");
                if (ecClient.GetAttributeValue<AliasedValue>("client.ssg_populationdesignation").Value.ToString().Substring(0, 2) == "PC")
                    enDT.Attributes.Add("ssg_popdesignation", "PC");
            }

            if (ecClient.Contains("client.ssg_dualbunkable"))
                enDT.Attributes.Add("ssg_dualbunkable", (OptionSetValue)ecClient.GetAttributeValue<AliasedValue>("client.ssg_dualbunkable").Value);

            if (ecClient.Contains("client.ssg_mentalhealthneeds"))
                enDT.Attributes.Add("ssg_mentalhealthneeds", (OptionSetValue)ecClient.GetAttributeValue<AliasedValue>("client.ssg_mentalhealthneeds").Value);

            if (ecClient.Contains("client.ssg_indigenous") && (bool)ecClient.GetAttributeValue<AliasedValue>("client.ssg_indigenous").Value == true)
                enDT.Attributes.Add("ssg_characterindigenous", new OptionSetValue(867670000));
            else if (ecClient.Contains("client.ssg_indigenous") && (bool)ecClient.GetAttributeValue<AliasedValue>("client.ssg_indigenous").Value == false)
                enDT.Attributes.Add("ssg_characterindigenous", new OptionSetValue(867670001));


            if (ecClient.Contains("client.ssg_lastcontinuousperiodofseparateconfinement") && ecClient.GetAttributeValue<AliasedValue>("client.ssg_lastcontinuousperiodofseparateconfinement") != null)
                enDT.Attributes.Add("ssg_continuousseparateconfinementid", (EntityReference)(ecClient.GetAttributeValue<AliasedValue>("client.ssg_lastcontinuousperiodofseparateconfinement").Value));
            else if (ecClient.Contains("ssg_separateconfinementperiodid") && ecClient.GetAttributeValue<Guid>("ssg_separateconfinementperiodid") != null)
                enDT.Attributes.Add("ssg_continuousseparateconfinementid", new EntityReference("ssg_separateconfinementperiod", ecClient.GetAttributeValue<Guid>("ssg_separateconfinementperiodid")));


            if (ecClient.Contains("ssg_celltype"))
                enDT.Attributes.Add("ssg_celltype", ecClient.GetAttributeValue<OptionSetValue>("ssg_celltype"));

            if (ecClient.Contains("ssg_client"))
            {
                var previousDT = previousDTCollection.Entities.Where(p => p.GetAttributeValue<EntityReference>("ssg_client").Id == ecClient.GetAttributeValue<EntityReference>("ssg_client").Id).Select(p => p).ToList();
                if (previousDT.Count > 0)
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
            }

            return enDT;
        }

    }
}
