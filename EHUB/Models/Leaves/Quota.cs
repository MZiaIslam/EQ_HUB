using EHUB.Models.HRManagement;

namespace EHUB.Models.Leaves
{
    public class Quota
    {
        public List<StaffData> staff { get; set; }
        public dynamic departs { get; set; }
        public dynamic leavetyps { get; set; }
    }
    public class LvQuota
    {
        public int mode { get; set; }
        public string[]? EmpIds { get; set; }
        public string[]? DepartIds { get; set; }
        public int yrQuota { get; set; }
        public int LeaveID { get; set; }
        public int Allowed { get; set; }
    }
    public class QuotaInfo
    {

        public int yrQuota { get; set; }
        public int Allowed { get; set; }
        public string Leave { get; set; }
     
        public string createdby { get; set; }
        public DateTime created { get; set; }
        public string? modifyby { get; set; } = null;
        public DateTime? modify { get; set; } = null;
        public int QuotaID { get; set; }
    }
}
