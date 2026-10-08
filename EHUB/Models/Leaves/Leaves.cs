using EHUB.Models.Administration;
using EHUB.Models.HRManagement;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHUB.Models.Leaves
{
	public class Leaves
	{
		public int LeaveID { get; set; }
		public int? EmpId { get; set; }
		public DateTime dtFrom { get; set; }
		public DateTime dtTo { get; set; }
		public string? Ldesc { get; set; }
		public string? attachment { get; set; }
		public int? LvId { get; set; } = null;
		public string? lvduration { get; set; }
		[NotMapped]
		public IFormFile? doc { set; get; } = null;
	}
	public class LvIds
	{
		public int LvId { get; set; }
	}
	public class LeaveType
	{
		public int? LeaveID { get; set; }
		public string? Leave { get; set; }
        public decimal? balance { get; set; }
    }
	public class LeaveLibrary
	{
		public List<LeaveType> leaveType { get; set; }
		public List<LeaveRequests> leaves { get; set; }
		public List<StaffData> staff { get; set; }
		public List<LeaveSumm> lvSumm { get; set; }
	}
	public class LeaveRequests
	{
		public int id { get; set; }
		public string? aprvstatus { get; set; }
		public DateOnly dtFrom { get; set; }
		public DateOnly dtTo { get; set; }
public DateTime? dtaprv { get; set; }= DateTime.MinValue;
		public string? Ldesc { get; set; }= null;
		public string? attachment { get; set; }=null;
		public string? Leave { get; set; } = null;
		public string? Mgr { get; set; } = null;
		public DateTime? Created { get; set; } = null;
		public string? lvduration { get; set; }

		public string? eName { get; set; }
        public string? aprvstat { get; set; }
        public int? station { get; set; }
        public int? jobstatus { get; set; }
        public int? leaveID { get; set; }
        public int? empid { get; set; }public int? subDepartment { get; set; }
    }

	public class LeaveSumm
	{
		public string? Leave { get; set; }
		public int? Allowed { get; set; }

        [DisplayFormat(DataFormatString = "{0:#.00")]
        public decimal? took { get; set; }
        [DisplayFormat(DataFormatString = "{0:#.00")]
        public decimal? bal { get; set; }
	}
}
