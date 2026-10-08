namespace EHUB.Models.Login
{
    public class LoginInfo
    {
        public string email { get; set; }
        public string pass { get; set; }
        public string name { get; set; }
        public int empid { get; set; }
        public int groupId { get; set; }
        public string? designation { get; set; }
        public string? depart { get; set; }
        public int? hwork { get; set; }
    }

}
