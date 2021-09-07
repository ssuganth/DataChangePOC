using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Crm.Sdk.Messages;


namespace DH_SeoCon_ScheduledJobs
{
    public class Program
    {
        
        static void Main(string[] args)
        {
            var sEntityToRefresh = ConfigurationManager.AppSettings["RefreshEntity"];
            var lastRefresh = Convert.ToDateTime(ConfigurationManager.AppSettings["LastRefreshedDate"]);


            RefreshSeedData_DH_SepCon dataRefreshClient = new RefreshSeedData_DH_SepCon(sEntityToRefresh, lastRefresh);
            dataRefreshClient.UpdateSeedData();



            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["LastExecutedDate"].Value = DateTime.Today.ToShortDateString();
            config.Save(ConfigurationSaveMode.Modified);

            ConfigurationManager.RefreshSection("appSettings");

        }

        

    }
}
