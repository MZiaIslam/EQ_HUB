using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;
namespace EHUB.Models.Administration
{
    public class GradeLevels
    {
        public int GradeID { get; set; }
        public string GradeLevel { get; set; }
    }
    public class SubjectList
    {
        public int SubjectID { get; set; }
        public string Subject { get; set; }
    }
    public class EmailMessage
    {
        public string Subject { get; set; }
        public string MailBody { get; set; }
        public IFormFile? Attachment { get; set; }
        // Multiple recipients
        public List<string> Recipients { get; set; } = new List<string>();
    }
    public class Departments
    {
        public int DepartID { get; set; }
        public string Depart { get; set; }
        public int? HeadEmp { get; set; }
        public int? CreatedBy { get; set; }
        public int? ModifyBy { get; set; }
        public int? ParentId { get; set; }
        [NotMapped]
        public Departments? SubDepartments { get; set; }
    }
    public class ShiftPlan
    {
        public int shiftId { get; set; }
        public string shiftName { get; set; }
        public DateTime? InTime { get; set; }
        public DateTime? OutTime { get; set; }
        public int? late { get; set; } = 0;
        public int? early { get; set; } = 0;
        public int? halfday { get; set; } = 0;
        public int? shortday { get; set; } = 0;
    }
    public class ShiftList
    {
        public int shiftId { get; set; }
        public string shiftName { get; set; }
    }
}
