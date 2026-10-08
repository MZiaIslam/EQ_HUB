namespace EHUB.Models.Administration
{
	public class Stations
	{
		public int StationID { get; set; }
		public string Station { get; set; }
		public int? City { get; set; }
		public string? Address { get; set; }
		public int? Stat { get; set; }
		public int? CreatedBy { get; set; }
		public int? ModifyBy { get; set; }
	}
    public class Grade
    {
        public int GradeID { get; set; }
        public string GradeLevel { get; set; }
    }
}
