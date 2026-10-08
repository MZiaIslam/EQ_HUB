namespace EHUB.Models.Administration
{
	public class WorkDay
	{

			public int mode { get; set; }
			public string[]? EmpIds { get; set; }
			public string[]? DepartIds { get; set; }
			public DateTime fdate { get; set; }
			public DateTime tdate { get; set; }
			public string state { get; set; }
		public string? descs { get; set; } = "";
		public int Recurring { get; set; }
		public int? option { get; set; }

	}
    public class MgrAssign
    {

        public int mode { get; set; }
        public string[]? EmpIds { get; set; }
        public string[]? DepartIds { get; set; }
        public int MgrId { get; set; }


    }
	public class aprLevel
	{
		public int[] item { get; set; }

    }
    public class WDayInfo
	{

		public string fdate { get; set; }
		public string tdate { get; set; }
		public string DayStatus { get; set; }
		public string Remarks { get; set; }
		public string createdby { get; set; }
		public DateTime created { get; set; }
		public string? modifyby { get; set; } = null;
		public DateTime? modify { get; set; } = null;
		public int id { get; set; }
	}

}
