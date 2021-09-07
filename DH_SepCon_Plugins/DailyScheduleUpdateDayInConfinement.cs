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
    public class DailyScheduleUpdateDayInConfinement : IPlugin
    {
        IOrganizationService _service;
        IPluginExecutionContext _context;       
        ITracingService trace;
        String _sExecutionBU = string.Empty;

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

            if (_context.InputParameters.Contains("businessunitid"))
                _sExecutionBU = _context.InputParameters["businessunitid"].ToString();

            try
            {
                trace.Trace("OnCreateConfinement - Begins  ");
                if (_sExecutionBU != String.Empty)
                {

                    #region Confinement Updates
                    //Fetch all the active Confinement for specific BU triggered from openshift to calculate Time Spent in Confinement
                    var fetchConfinement = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
                                "<entity name='ssg_separateconfinement'>" +
                                "<attribute name='createdon' />" +
                                "<attribute name='ssg_date' />" +
                                "<attribute name='ssg_inmateid' />" +
                                "<attribute name='ssg_number' />" +
                                "<attribute name='ssg_separateconfinementtype_t_' />" +
                                "<attribute name='ownerid' />" +
                                "<attribute name='statuscode' />" +
                                "<attribute name='ssg_separateconfinementid' />" +
                                "<attribute name='statecode' />" +
                                "<attribute name='ssg_actualenddatetime' />" +
                                "<attribute name='ssg_separateconfinementenddate' />" +
                                "<order attribute='ssg_inmateid' descending='false' />" +
                                "<filter type='and'>" +
                                "<condition attribute='ssg_correctionalcentre' operator='eq' value='" + _sExecutionBU + "' />" +
                                //"<condition attribute='ssg_separateconfinementid' value='d853998e-6dbe-eb11-b82c-00505683d8c0' operator='eq'/>" +
                                "<condition attribute='statecode' value='0' operator='eq'/>" +
                                "</filter>" +
                                "</entity>" +
                                "</fetch>";

                    trace.Trace("_sExecutionBU: " + _sExecutionBU);

                    EntityCollection ecConfinement = _service.RetrieveMultiple(new FetchExpression(fetchConfinement));
                    trace.Trace(" DailyScheduleUpdateDayInConfinement: Count of Confinement " + ecConfinement.Entities.Count().ToString());
                    //if there are active confinements, update confinement's timespent in days field
                    if (ecConfinement.Entities.Count() > 0)
                    {
                        foreach (var confinement in ecConfinement.Entities)
                        {
                            if (confinement.Contains("ssg_date") ) 
                            {
                                Entity updateConf = new Entity("ssg_separateconfinement");
                                trace.Trace(" DailyScheduleUpdateDayInConfinement: Start of Confinement " + confinement.GetAttributeValue<DateTime>("ssg_date").Date.ToString());
                                trace.Trace(" DailyScheduleUpdateDayInConfinement: Today " + DateTime.Today.Date.ToString());
                                TimeSpan ts = new TimeSpan(00, 00, 0);
                                var startDate = confinement.GetAttributeValue<DateTime>("ssg_date").ToLocalTime();
                                var iConfinementDays = Convert.ToInt32((DateTime.Today.Date - startDate.Date).TotalDays + 1);

                                trace.Trace(" DailyScheduleUpdateDayInConfinement: iConfinementDays " + iConfinementDays);
                                
                                updateConf.Attributes.Add("ssg_timespentindaysvalue", iConfinementDays);
                                
                                updateConf.Attributes.Add("ssg_separateconfinementid", confinement.Id);
                                _service.Update(updateConf);
                            }
                        }
                    }

                    #endregion

                    #region CTOC Updates
                    //fetch all the active CTOC's for specific BU triggered from openshift
                    var fetchActiveCTOC = "<fetch distinct='true' mapping='logical' output-format='xml-platform' version='1.0'>" +
                                                "<entity name='ssg_separateconfinementperiod'>" +
                                                "<attribute name='ssg_separateconfinementperiodid'/>" +
                                                "<attribute name='ssg_name'/>" +
                                                "<attribute name='ssg_client'/>" +
                                                "<attribute name='ssg_csnumber'/>" +
                                                "<attribute name='ssg_correctioncentre'/>" +
                                                "<attribute name = 'ssg_confinementstartdate'/>" +
                                                "<filter type='and'>" +
                                                "<condition attribute='statecode' value='0' operator='eq'/>" +
                                                "<condition attribute='ssg_correctioncentre' value='"+ _sExecutionBU +"' operator='eq'/>" +
                                                //"<condition attribute='ssg_separateconfinementperiodid' value='257bD853998E-6DBE-EB11-B82C-00505683D8C0' operator='eq'/>" +
                                                "</filter>" +
                                                "</entity>" +
                                                "</fetch>";

                    EntityCollection ecActiveCTOC = _service.RetrieveMultiple(new FetchExpression(fetchActiveCTOC));
                    trace.Trace(" DailyScheduleUpdateDayInConfinement: Count of CTOC " + ecActiveCTOC.Entities.Count().ToString());
                    if (ecActiveCTOC.Entities.Count > 0)
                    {
                        //loop through all the CTOC's and update consecutive days in confinement
                        foreach (var eCTOC in ecActiveCTOC.Entities)
                        {

                            if (eCTOC.Contains("ssg_confinementstartdate"))
                            {
                                TimeSpan ts = new TimeSpan(00, 00, 0);
                                //temp fix - reducing 8 hours as rollup field retrieves date as GMT. But the calculation should be in userlocal.
                                var startDate = eCTOC.GetAttributeValue<DateTime>("ssg_confinementstartdate").ToLocalTime();

                                var iConfinementDays = Convert.ToInt32((DateTime.Today - startDate.Date).TotalDays + 1);

                                var updateCTOC = new Entity("ssg_separateconfinementperiod");
                                updateCTOC.Attributes.Add("ssg_consecutivedaysinconfinement", iConfinementDays);
                                updateCTOC.Attributes.Add("ssg_separateconfinementperiodid", eCTOC.Id);
                                _service.Update(updateCTOC);
                            }
                        }
                    }




                    #endregion
                }



                trace.Trace("OnCreateConfinement - Ends  ");

            }
            catch (Exception e)
            {
                throw new InvalidPluginExecutionException("DailyScheduleUpdateDaysInConfinement Plugin: " + e.Message);
            }


        }
    }
}
