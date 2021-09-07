using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DH_SepCon_Plugins
{
    public class OnCreateConfinement:IPlugin
    {
        IOrganizationService _service;
        IPluginExecutionContext _context;

        public void Execute(IServiceProvider serviceProvider)
        {
            _context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            if (_context.InputParameters.Contains("Target") &&
                        _context.InputParameters["Target"] is Entity)
            {
                // Obtain the target entity from the input parameters.  
                Entity entity = (Entity)_context.InputParameters["Target"];

                // Obtain the organization service reference which you will need for  
                // web service calls.  
                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                _service = serviceFactory.CreateOrganizationService(_context.UserId);
                ITracingService trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

                Entity updateConf = new Entity("ssg_separateconfinement");
                //calculate time spent in days on create of Confinement
                if (entity.Contains("ssg_date") && entity.Contains("ssg_actualenddatetime"))
                {
                    //if confinement has actual end date, then consider that for calculation
                    trace.Trace("OnCreateConfinement - Start Date - " + entity.GetAttributeValue<DateTime>("ssg_date").Date.ToString());
                    trace.Trace("OnCreateConfinement - Actual End Date - " + entity.GetAttributeValue<DateTime>("ssg_date").Date.ToString());
                    var iConfinementDays = Convert.ToInt32((entity.GetAttributeValue<DateTime>("ssg_actualenddatetime").Date - entity.GetAttributeValue<DateTime>("ssg_date").Date).TotalDays + 1);
                    updateConf.Attributes.Add("ssg_timespentindaysvalue",iConfinementDays);
                    trace.Trace("OnCreateConfinement - IConfinement - " + iConfinementDays.ToString());
                    updateConf.Attributes.Add("ssg_separateconfinementid", entity.Id);
                    // _service.Update(updateConf);
                    entity["ssg_timespentindaysvalue"] = iConfinementDays;
                }
                else if(entity.Contains("ssg_date"))
                {
                    trace.Trace("OnCreateConfinement - " + entity.GetAttributeValue<DateTime>("ssg_date").Date.ToString());
                    trace.Trace("OnCreateConfinement - Today - " + DateTime.Today.ToString());
                    
                    var iConfinementDays = Convert.ToInt32((DateTime.Today - entity.GetAttributeValue<DateTime>("ssg_date").Date).TotalDays + 1);
                    trace.Trace("OnCreateConfinement - IConfinement - " + iConfinementDays.ToString());
                    updateConf.Attributes.Add("ssg_timespentindaysvalue", iConfinementDays);
                    updateConf.Attributes.Add("ssg_separateconfinementid", entity.Id);
                    //_service.Update(updateConf);
                    entity["ssg_timespentindaysvalue"] = iConfinementDays;

                }


            }
        }
    }
}
