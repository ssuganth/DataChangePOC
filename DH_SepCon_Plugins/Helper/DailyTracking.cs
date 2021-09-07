using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DH_SepCon_Plugins.Helper
{
    public class DailyTracking
    {
        public EntityReference UnitCell { get; set; }

        public EntityReference CorrectionalCenter { get; set; }
        public OptionSetValue CGICRating { get; set; }

        public Boolean FifteenMinCheck { get; set; }

        public Boolean SpecialHandlingProtocol { get; set; }
        public Boolean CasePlanCompleted { get; set; }
        public String CSNumber { get; set; }
        public String SecurityCautionsAlertsNotes { get; set; }
        public Guid PreviousDT { get; set; }

        public String PopulationDesignation { get; set; }


        public String CellCallBox { get; set; }
        public String CellCamNumber { get; set; }
        public String CellLocation { get; set; }
        public int CellType { get; set; }
       
        public Guid Client { get; set; }

       

        public Guid CTOC { get; set; }
        
        public DateTime ssgDate { get; set; }

       

        public string PublicCautionsAlert { get; set; }

        

        

    }
}
