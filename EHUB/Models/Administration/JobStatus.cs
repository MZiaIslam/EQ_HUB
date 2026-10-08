namespace EHUB.Models.Administration
{
	public class JobStatus
	{
		public int Id { get; set; }
		public string EmployeeStatus { get; set; }
		public int? CreatedBy { get; set; }
		public int? ModifyBy { get; set; }
	}
    public class Libs
    {
        
        public int? libId { get; set; }
        public int? LiBType { get; set; }
public string? LibName { get; set; }
        public int? CreatedBy { get; set; }
        public int? ModifyBy { get; set; }
		public string? cls { get; set; }
	}
}
