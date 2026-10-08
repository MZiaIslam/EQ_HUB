using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace EHUB.Models.HRManagement
{
    public class TutoringSession
    {
        public int StudentID { get; set; }
        public int GradeID { get; set; }
        public double PurchasedHrs { get; set; }
        public double TotalAmt { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int[]? Subjects { get; set; }
        public int? TutorID { get; set; }
        public int? SessionId { get; set; }
        public int? Recurrence { get; set; }
        public int? Scheduled { get; set; } = 0;
        public int? Completed { get; set; } = 0;
        public int? Cancel { get; set; } = 0;
    }
    public class StudentProfile
    {
        public int? StudentID { get; set; } = 0;
        public string StudentName { get; set; }
        public string? Guardian { get; set; }
        public string? CellNo { get; set; }
        public string? Email { get; set; }
        public string? Relationship { get; set; }
        public DateTime? DOB { get; set; }
        public int? GradeID { get; set; }
        public string? School { get; set; }
        public string? TutoringFormat { get; set; }
        public string? PreferredSchedule { get; set; }
        public string? SessionsFrequency { get; set; }
        public string? HealthConcerns { get; set; }
        public string? EmergencyContact { get; set; }
        public string? Subjects { get; set; }
        public string? AcadPerformance { get; set; }
        public string? LearningGoals { get; set; }
        public string? Gender { get; set; }
        public string? Stat { get; set; }
        public string? GradeLevel { get; set; }
        public double? PurchasedHrs { get; set; }
        public double? TotalAmt { get; set; }
        public DateOnly AppDate { get; set; }
        [NotMapped]
        public IFormFile? photo { set; get; } = null;
    }
    public class NewStudent
    {
        public int? StudentID { get; set; } = 0;
        public string StudentName { get; set; }
        public string? Guardian { get; set; }
        public string? CellNo { get; set; }
        public string? Email { get; set; }
        public string? Relationship { get; set; }
        public DateTime? DOB { get; set; }
        public int? GradeID { get; set; }
        public string? School { get; set; }
        public string? TutoringFormat { get; set; }
        public string? PreferredSchedule { get; set; }
        public string? SessionsFrequency { get; set; }
        public string? HealthConcerns { get; set; }
        public string? EmergencyContact { get; set; }
        public string? Subjects { get; set; }
        public string? AcadPerformance { get; set; }
        public string? LearningGoals { get; set; }
public string? Gender { get; set; }
        public string? Stat { get; set; }
        public DateOnly AppDate { get; set; }
        [NotMapped]
        public IFormFile? photo { set; get; } = null;
    }
    public class Empolyee
    {
        public int? EmpID { get; set; } = 0;
        public int EmpCode { get; set; }
        public string FName { get; set; }
        public string? LName { get; set; } = "";
        public string? FatherName { get; set; } = null;
        public string? MobileNo { get; set; } = "";
        public string Email { get; set; } = "";
      
        public string? Marital { get; set; } = "Single";
        public string? Gender { get; set; } = "Male";
        public DateTime? DateOfBirth { get; set; }
       
     
        
        public string? ContactPerson { get; set; } = "";
        public string? Relationship { get; set; } = "";
        public string? ContactNo { get; set; } = "";
        public string? Address { get; set; } = "";
        public string? permanentaddress { get; set; } = "";
        
        public int? Designation { get; set; }
        
        public DateTime? JoiningDate { get; set; }
        
        public int? GroupID { get; set; }
        
        
        public string? remarks { get; set; } = "";
public int? EmpStatus { get; set; } = 0;
        [NotMapped]
        public int[]? LocationIds { get; set; }
        [NotMapped]
        public IFormFile? photo { set; get; } = null;
    }
    public class EmpIds
    {
        public int EmpID { get; set; }
    }
    public class orgchart
    {
        public int id { get; set; }
        public string? name { get; set; }
        public string? title { get; set; }
        public int? pid { get; set; }
        public string? email { get; set; }
        public string? img { get; set; }
    }
    public class EmpEditLog
    {
        public int EmpCode { get; set; }
        public string? FullName { get; set; }
        public string? FatherName { get; set; }
        public string? MobileNo { get; set; }
        public string? Email { get; set; }
        public string? LineMgr { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Station { get; set; }
        public string? Depart { get; set; }
        public string? Designation { get; set; }
        public DateTime? Modify { get; set; }
        public string? ModifyBy { get; set; }
        public string? EmployeeStatus { get; set; }
        public string? Stat { get; set; }
        public DateTime? tDate { get; set; }
        public string? Descs { get; set; }


     
    }
}
