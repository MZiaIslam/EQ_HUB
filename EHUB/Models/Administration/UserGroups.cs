using System.ComponentModel.DataAnnotations.Schema;

namespace EHUB.Models.Administration
{
	public class UserGroups
	{
		public int GroupID { get; set; }
		public string GroupName { get; set; }
		public int? CreatedBy { get; set; }
		public int? ModifyBy { get; set; }
	}
	public class PageList
	{
		public int pgid { get; set; }
		public string pgname { get; set; }
		public int roleid { get; set; }
		public int pglavel { get; set; }
		public int tblid { get; set; }
		public string icon { get; set; }
	}
}