namespace EHUB.Utilities
{
	public class Locations
	{
		public int LocId { get; set; }
		public string Loc { get; set; }
		public int? pLocId { get; set; }
		public int? LocType { get; set; }
		public int? CreatedBy { get; set; }
		public int? ModifyBy { get; set; }
	}
	public class Desigs
	{
		public int Id { get; set; }
		public string Designation { get; set; }
		public int? CreatedBy { get; set; }
		public int? ModifyBy { get; set; }
	}
}
