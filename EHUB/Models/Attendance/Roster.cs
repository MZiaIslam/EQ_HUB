using EHUB.Models.HRManagement;
using EHUB.Models.Leaves;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Data;
using System.Net.NetworkInformation;

namespace EHUB.Models.Attendance
{
	public class Roster
	{
		public int mode { get; set; }
		public string[]? EmpIds { get; set; }
		public string[]? DepartIds { get; set; }
		public DateTime fdate { get; set; }
		public DateTime tdate { get; set; }
		public DateTime intime { get; set; }
		public DateTime outtime { get; set; }
	}
	public class pgRoster
	{
		public List<StaffData> staff { get; set; }
		public dynamic departs { get; set; }
		public List<ExemptTypes>? exptTypes { get; set; }
public List<DataRow>? design { get; set; }
	}
	public class RosterInfo
	{
		
		public string fdate { get; set; }
		public string tdate { get; set; }
		public string intime { get; set; }
		public string outtime { get; set; }
		public int rstatus { get; set; }
		public string createdby { get; set; }
		public DateTime created { get; set; }
		public string? modifyby { get; set; } = null;
		public DateTime? modify { get; set; } = null;
		public int id { get; set; }
	}
}
