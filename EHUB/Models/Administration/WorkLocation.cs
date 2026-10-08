namespace EHUB.Models.Administration
{
	public class WorkLocation
	{
		public int LocationId { get; set; }
		public string LocationName { get; set; }
		public string? Address { get; set; }
		public string? City { get; set; }
		public string? Phone { get; set; }
		public bool IsActive { get; set; } = true;
	}
	public class WorkLocationRow : WorkLocation
	{
		public int StaffCount { get; set; }
	}
}
