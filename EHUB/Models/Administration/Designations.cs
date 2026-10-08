namespace EHUB.Models.Administration
{
	public class Designations
	{
		public int Id { get; set; }
		public string Designation { get; set; }
		public int? CreatedBy { get; set; }
		public int? ModifyBy { get; set; }
	}
}
