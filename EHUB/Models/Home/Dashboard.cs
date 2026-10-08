using EHUB.Models.HRManagement;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace EHUB.Models.Home
{
    public class Dash
    {
        public List<SelectListItem>? emps { get; set; }
    }
    public class AttCalender
    {
        public int? leave { get; set; } = null;
        public string? descs { get; set; } = null;
        public DateTime? lday { get; set; }
        public int cdy { get; set; }
        public string? cls { get; set; } = null;
    }
    public class AttChart
    {
        public DateTime? dtatt { get; set; }
        public DateTime? intime { get; set; } = null;
        public DateTime? otime { get; set; } = null;
        public int? latein { get; set; } = null;
        public double? whours { get; set; } = null;
        public string? lvcat { get; set; } = null; public string? stat { get; set; }
        public int? lateout { get; set; } = null;
        public string? timein { get; set; } = null;
        public string? outtime { get; set; } = null;
        public string? thours { get; set; } = null;
        public string? lvstat { get; set; } = null;
        public string? color { get; set; } = null;
    }
    public class AttSumm
    {
        public string? name { get; set; } = null;
        public string? color { get; set; } = null;
        public int? y { get; set; }
        public int? thours { get; set; }
    }
    public class OverallAtt
    {
        public int? total { get; set; }
        public int? present { get; set; }
        public int? onleave { get; set; }
        public int? absents { get; set; }
        public int? onattand { get; set; }
    }
    public class TeamAttend
    {
        public DateTime? dtatt { get; set; } = null;
        public TimeOnly? intime { get; set; } = null;
        public TimeOnly? otime { get; set; } = null;
        public int? latein { get; set; } = null;
        public int? lateout { get; set; } = null;
        public int? whours { get; set; } = null;
        public string? lvcat { get; set; } = null;
        public string? stat { get; set; } = null;
        public string? color { get; set; } = null;
        public string? timein { get; set; } = null;
        public string? outtime { get; set; } = null;
        public string? thours { get; set; } = null;
        public string? lvstat { get; set; } = null;
        public int? empid { get; set; } = null;
        public int? EmpCode { get; set; } = null;
        public string? FName { get; set; } = null;
        public string? LName { get; set; } = null;
        public string? Designation { get; set; } = null;
    }

    public class SessionSchedules
    {
        public string StudentName { get; set; }
        public string Guardian { get; set; }
        public string Relationship { get; set; }
        public string Subjects { get; set; }
        public string Tutor { get; set; }
        public string Stat { get; set; }
        public string? Email { get; set; }public string? empMail { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int id { get; set; }
        public int points { get; set; }public int? TutorID { get; set; }
    }
    public class feedbacks
    {
        public int CreatedBy { get; set; }
        public string name { get; set; }
        public DateTime Created { get; set; }
        public string filepath { get; set; }
        public string details { get; set; }
        public string utype { get; set; }
        public int id { get; set; }

    }
    public class LowSessionNotification
    {
        public decimal ScheduledHours { get; set; }
        public int StudentID { get; set; }
        public int SessionCount { get; set; }
        public string StudentName { get; set; }
        public string Guardian { get; set; }
        public int SessionID { get; set; }
        public string Relationship { get; set; }
        public string CellNo { get; set; }
    }
    public class fback
    {
        public string details { get; set; }
        public IFormFile? Attachment { get; set; }

    }
    public class bag
    {
        public SessionSchedules? SessionSch { get; set; }
        public List<feedbacks>? feedbacks { get; set; }
        public List<SelectListItem>? emps { get; set; }
        public int? SelectedEmpId { get; set; }
    }
}
