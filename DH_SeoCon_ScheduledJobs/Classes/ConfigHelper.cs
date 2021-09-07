using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DH_SeoCon_ScheduledJobs.Classes
{
    public class ConfigHelper
    {
        public static string fetchConfinement = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
                                    "<entity name='ssg_separateconfinement'>" +
                                    "<attribute name='ssg_date'/>" +
                                    "<attribute name='ssg_separateconfinementenddate'/>" +
                                    "<attribute name='ssg_separateconfinementid'/>" +
                                    "<attribute name='ssg_actualenddatetime'/>" +
                                    "<attribute name='ssg_s17starteddate'/>" +
                                    "<attribute name='ssg_extensioncreateddate'/>" +
                                    "<attribute name='ssg_originalenddatetime'/>" +
                                    "<attribute name='ssg_newendtime'/>" +
                                    "<attribute name='ssg_initiationdate'/>" +
                                    "<attribute name='ssg_confinementcreationdate'/>" +
                                    "<attribute name='ssg_dateofnotification'/>" +
                                    "<attribute name='ssg_approveddate'/>" +
                                    "<order attribute='ssg_date' descending='false'/>" +
                                    "<filter type='and'>" +
                                    "<condition attribute='createdon' operator='on-or-after' value='2021-05-27'/>" +
                                    "<condition attribute='modifiedon' operator='on-or-before' value='" + ConfigurationManager.AppSettings["LastExecutedDate"].ToString() + "'/>" +
                                    "</filter>" +
                                    "</entity>" +
                                    "</fetch>";


        public static string fetchCTOC = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
                            "<entity name='ssg_separateconfinementperiod'>" +
                            "<attribute name='ssg_currentlastconfinementexpiry'/>" +
                            "<attribute name='ssg_confinementstartdate'/>" +
                            "<attribute name='ssg_separateconfinementperiodid'/>" +
                            "<attribute name='ssg_nextconfperiodexpirydate'/>" +
                            "<attribute name='ssg_mhreviewduedate'/>" +
                            "<attribute name='ssg_s17expirydatetime'/>" +
                            "<attribute name='ssg_dwreviewduedate'/>" +
                            "<attribute name='ssg_actualconfinementend'/>" +
                            "<attribute name='ssg_endforopenstatuscalc'/>" +
                            "<attribute name='ssg_currentconfinementperiodstartdate'/>" +
                            "<attribute name='ssg_hqreviewduedate'/>" +
                            "<order attribute='ssg_confinementstartdate' descending='false'/>" +
                            "<filter type='and'>" +
                            "<condition attribute='createdon' operator='on-or-after' value='2021-05-27'/>" +
                             "<condition attribute='modifiedon' operator='on-or-before' value='" + ConfigurationManager.AppSettings["LastExecutedDate"].ToString() + "'/>" +
                            "</filter>" +
                            "</entity>" +
                            "</fetch>";

        public static string fetchReviews = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
                                "<entity name='ssg_separateconfinementreview'>" +
                                "<attribute name='ssg_reviewduedate'/>" +
                                "<attribute name='ssg_separateconfinementreviewid'/>" +
                                "<attribute name='ssg_reviewcompletiondatetime'/>" +
                                "<order attribute='ssg_reviewduedate' descending='false'/>" +
                                "<filter type='and'>" +
                                "<condition attribute='createdon' operator='on-or-after' value='2021-05-27'/>" +
                                "<condition attribute='modifiedon' operator='on-or-before' value='" + ConfigurationManager.AppSettings["LastExecutedDate"].ToString() + "'/>" +
                                "</filter>" +
                                "</entity>" +
                                "</fetch>";

        public static string fetchDailyTracking = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
                                    "<entity name='ssg_dailytracking'>" +
                                    "<attribute name='ssg_date'/>" +
                                    "<attribute name='ssg_dailytrackingid'/>" +
                                    "<attribute name='ssg_managerreviewtime'/>" +
                                    "<attribute name='ssg_healthcareconsulttime'/>" +
                                    "<attribute name='ssg_name'/>" +
                                    "<order attribute='ssg_date' descending='true'/>" +
                                    "<filter type='and'>" +
                                    "<condition attribute='createdon' operator='on-or-after' value='2021-05-27'/>" +
                                     "<condition attribute='modifiedon' operator='on-or-before' value='" + ConfigurationManager.AppSettings["LastExecutedDate"].ToString() + "'/>" +
                                    "</filter>" +
                                    "</entity>" +
                                    "</fetch>";

        public static string fetchLeaves = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
                                "<entity name='ssg_leavevisit'>" +
                                "<attribute name='ssg_leavevisitid'/>" +
                                "<attribute name='createdon'/>" +
                                "<attribute name='ssg_start'/>" +
                                "<attribute name='ssg_end'/>" +
                                "<order attribute='ssg_end' descending='false'/>" +
                                "<filter type='and'>" +
                                "<condition attribute='createdon' operator='on-or-after' value='2021-05-27'/>" +
                                "<condition attribute='modifiedon' operator='on-or-before' value='" + ConfigurationManager.AppSettings["LastExecutedDate"].ToString() + "'/>" +
                                "</filter>" +
                                "</entity>" +
                                "</fetch>";

        public static string fetchClient = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
                                "<entity name='contact'>" +
                                "<attribute name='contactid'/>" +
                                "<attribute name='ssg_24hoursreactivationdeadline'/>" +
                                "<order attribute='ssg_24hoursreactivationdeadline' descending='false'/>" +
                                "<filter type='and'>" +
                                "<condition attribute='modifiedon' operator='on-or-after' value='2021-05-27'/>" +
                                "<condition attribute='ssg_24hoursreactivationdeadline' operator='not-null'/>"+
                                "<condition attribute='modifiedon' operator='on-or-before' value='" + ConfigurationManager.AppSettings["LastExecutedDate"].ToString() + "'/>" +
                                "</filter>" +
                                "</entity>" +
                                "</fetch>";
    }
}
