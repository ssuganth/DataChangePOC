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
using DH_SeoCon_ScheduledJobs.Classes;
using System.ServiceModel.Channels;


namespace DH_SeoCon_ScheduledJobs
{
    public class RefreshSeedData_DH_SepCon
    {
        String _sEntityToRefresh = String.Empty;
        DateTime _dtLastRefreshedDate = DateTime.Today.AddDays(-1);
        EntityCollection ecToBeUpdated;
        IOrganizationService _crmService = null;
        public RefreshSeedData_DH_SepCon(String EntityToRefresh, DateTime LastRefreshedDate)
        {
            _sEntityToRefresh = EntityToRefresh;
            _dtLastRefreshedDate = LastRefreshedDate;
        }

        public void UpdateSeedData()
        {
            Double iDateDifference = (DateTime.Today - _dtLastRefreshedDate).TotalDays;
            //QueryExpression GetRecords = new QueryExpression()
            //{
            //    EntityName = ConfigurationManager.AppSettings["RetrieveEntity"],
            //    //ColumnSet = new ColumnSet(ConfigurationManager.AppSettings["RetriveColumn"]),
            //    Criteria =
            //        {
            //            FilterOperator = LogicalOperator.And,
            //            Conditions =
            //            {
            //                new ConditionExpression("createdon", ConditionOperator.OnOrAfter, "2021-05-27"),
            //                new ConditionExpression("modifiedon", ConditionOperator.OnOrBefore, ConfigurationManager.AppSettings["LastExecutedDate"])
            //            }
            //        }
            //};
            //ColumnSet columns2 = new ColumnSet();
            //foreach (var value in ConfigurationManager.AppSettings["RetriveColumn"].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            //{

               
            //    columns2.AddColumn(value);
                


            //}

            //GetRecords.ColumnSet = columns2;

           
            //using (CrmServiceClient crmSvc1 = new CrmServiceClient(ConfigurationManager.ConnectionStrings["MyCRMServer"].ConnectionString))
            //{
            //    IOrganizationService crmService1 = (IOrganizationService)crmSvc1.OrganizationWebProxyClient != null ? (IOrganizationService)crmSvc1.OrganizationWebProxyClient : (IOrganizationService)crmSvc1.OrganizationServiceProxy;
            //    ecToBeUpdated = crmService1.RetrieveMultiple(GetRecords);
            //}
            



            
            if (_sEntityToRefresh == "Confinement")
            {
                using (CrmServiceClient crmSvc1 = new CrmServiceClient(ConfigurationManager.ConnectionStrings["MyCRMServer"].ConnectionString))
                {
                    IOrganizationService crmService1 = (IOrganizationService)crmSvc1.OrganizationWebProxyClient != null ? (IOrganizationService)crmSvc1.OrganizationWebProxyClient : (IOrganizationService)crmSvc1.OrganizationServiceProxy;
                    ecToBeUpdated = crmService1.RetrieveMultiple(new FetchExpression(ConfigHelper.fetchConfinement));
                }             

                
            }
            else if (_sEntityToRefresh == "CTOC")
            {
                using (CrmServiceClient crmSvc1 = new CrmServiceClient(ConfigurationManager.ConnectionStrings["MyCRMServer"].ConnectionString))
                {
                    IOrganizationService crmService1 = (IOrganizationService)crmSvc1.OrganizationWebProxyClient != null ? (IOrganizationService)crmSvc1.OrganizationWebProxyClient : (IOrganizationService)crmSvc1.OrganizationServiceProxy;
                    ecToBeUpdated = crmService1.RetrieveMultiple(new FetchExpression(ConfigHelper.fetchCTOC));
                }               


            }
            else if (_sEntityToRefresh == "Reviews")
            {
                
                using (CrmServiceClient crmSvc1 = new CrmServiceClient(ConfigurationManager.ConnectionStrings["MyCRMServer"].ConnectionString))
                {
                    IOrganizationService crmService1 = (IOrganizationService)crmSvc1.OrganizationWebProxyClient != null ? (IOrganizationService)crmSvc1.OrganizationWebProxyClient : (IOrganizationService)crmSvc1.OrganizationServiceProxy;
                    ecToBeUpdated = crmService1.RetrieveMultiple(new FetchExpression(ConfigHelper.fetchReviews));
                }


            }
            else if (_sEntityToRefresh == "DailyTracking")
            {
                
                using (CrmServiceClient crmSvc1 = new CrmServiceClient(ConfigurationManager.ConnectionStrings["MyCRMServer"].ConnectionString))
                {
                    IOrganizationService crmService1 = (IOrganizationService)crmSvc1.OrganizationWebProxyClient != null ? (IOrganizationService)crmSvc1.OrganizationWebProxyClient : (IOrganizationService)crmSvc1.OrganizationServiceProxy;
                    ecToBeUpdated = crmService1.RetrieveMultiple(new FetchExpression(ConfigHelper.fetchDailyTracking));
                }


            }
            else if (_sEntityToRefresh == "Leaves")
            {
             
                using (CrmServiceClient crmSvc1 = new CrmServiceClient(ConfigurationManager.ConnectionStrings["MyCRMServer"].ConnectionString))
                {
                    IOrganizationService crmService1 = (IOrganizationService)crmSvc1.OrganizationWebProxyClient != null ? (IOrganizationService)crmSvc1.OrganizationWebProxyClient : (IOrganizationService)crmSvc1.OrganizationServiceProxy;
                    ecToBeUpdated = crmService1.RetrieveMultiple(new FetchExpression(ConfigHelper.fetchLeaves));
                }



            }
            else if (_sEntityToRefresh == "Client")
            {

                using (CrmServiceClient crmSvc1 = new CrmServiceClient(ConfigurationManager.ConnectionStrings["MyCRMServer"].ConnectionString))
                {
                    IOrganizationService crmService1 = (IOrganizationService)crmSvc1.OrganizationWebProxyClient != null ? (IOrganizationService)crmSvc1.OrganizationWebProxyClient : (IOrganizationService)crmSvc1.OrganizationServiceProxy;
                    ecToBeUpdated = crmService1.RetrieveMultiple(new FetchExpression(ConfigHelper.fetchClient));
                }
                


            }

            if (ecToBeUpdated.Entities.Count > 0)
            {
                foreach (var enLoopThrough in ecToBeUpdated.Entities)
                {
                    var enToBeUpdated = enLoopThrough;
                    foreach (var att in enLoopThrough.Attributes.ToList())
                    {

                        if (_sEntityToRefresh == "DailyTracking")
                        {
                            if (!att.Key.Contains("id") && att.Key != "ssg_name" && att.Key!="ssg_date")
                            {
                                enToBeUpdated[att.Key] = Convert.ToDateTime(enLoopThrough[att.Key]).AddDays(iDateDifference);
                            }
                            else if (!att.Key.Contains("id") && att.Key != "ssg_name" && att.Key == "ssg_date")
                            {
                                enToBeUpdated[att.Key] = Convert.ToDateTime(enLoopThrough[att.Key]).AddDays(iDateDifference).ToLocalTime();
                            }
                        }
                        else if (_sEntityToRefresh != "DailyTracking" && !att.Key.Contains("id") && att.Key != "ssg_name")
                        {
                            enToBeUpdated[att.Key] = Convert.ToDateTime(enLoopThrough[att.Key]).AddDays(iDateDifference);
                        }


                    }

                    if (_sEntityToRefresh == "DailyTracking")
                    {
                        enToBeUpdated["ssg_name"] = enToBeUpdated["ssg_date"].ToString();
                    }
                    //enToBeUpdated["overriddencreatedon"] = new DateTime(2012, 2, 22);

                    
                    _crmService.Update(enToBeUpdated);


                }
            }            

        }
    }
}
