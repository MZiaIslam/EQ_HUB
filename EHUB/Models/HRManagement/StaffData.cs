using EHUB.Models.Administration;
using EHUB.Models.Leaves;
using EHUB.Utilities;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.SqlServer.Server;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Collections.Specialized.BitVector32;
namespace EHUB.Models.HRManagement
{
    public class Student
    {
        public int StudentID { get; set; }
        public string StudentName { get; set; }
        public string? Guardian { get; set; }
        public string? CellNo { get; set; }
        public string? Email { get; set; }
        public string? Relationship { get; set; }
        public DateTime DOB { get; set; }
        public int GradeID { get; set; }
        public string? School { get; set; }
        public string? TutoringFormat { get; set; }
        public string? PreferredSchedule { get; set; }
        public string? SessionsFrequency { get; set; }
        public string? HealthConcerns { get; set; }
        public string? EmergencyContact { get; set; }
        public string? Subjects { get; set; }
        public string? AcadPerformance { get; set; }
        public string? LearningGoals { get; set; }
        public string? Stat { get; set; }
        public DateOnly AppDate { get; set; }
        public string? Gender { get; set; }
    }
    public class Subjects
    {
        
        public int SubjectID { get; set; }
        public string? Subject { get; set; }
    }
    public class StaffData
    {
        public int empid { get; set; }
        public int? empcode { get; set; }
        public string? empname { get; set; }
        public string? fathername { get; set; }
        public string? mobileno { get; set; }
        public string? email { get; set; }
        public string? address { get; set; }
        public string? cnicno { get; set; }
        public string? station { get; set; }
        public string? stationaddr { get; set; }
        public string? loc { get; set; }
        public string? designation { get; set; }
        public string? depart { get; set; }
        public int empstatus { get; set; }
        public DateOnly? joiningdate { get; set; }
        public string? employeestatus { get; set; }
        public string? linemanager { get; set; }
        public int? isAtt { get; set; }
        public int? RManager { get; set; }
        public int? jobstatus { get; set; }
        public int? stationid { get; set; }public int? subDepartment { get; set; }
        public int? manualAttendance { get; set; }
        public int? parentId { get; set; }
    }
    public class fillters
    {
        public List<SelectListItem>? stations { get; set; }
        public List<SelectListItem>? emps { get; set; }
        public List<StaffData>? linemgr { get; set; }
        public List<SelectListItem>? jobstat{ get; set; }
        public List<SelectListItem>? subdepart { get; set; }
        public List<SelectListItem>? depart { get; set; }
        public Fillterdata? fills { get; set; }
        public List<LeaveRequests>? leaves { get; set; }
        public List<SelectListItem>? leavetypes { get; set; }
        public List<Student>? students { get; set; }
    }
    public class Fillterdata
    {
        public string? station { get; set; }
        public string? depart { get; set; }
        public string? subdepart { get; set; }
        public string? empstat { get; set; }
        public string? emp { get; set; }
        public string? chkdt { get; set; }
        public DateOnly? fromdate { get; set; }
        public DateOnly? todate { get; set; }
        public string? ManualAttendance { get; set; } public string? LeaveStatus { get; set; }
        public string? LeaveType { get; set; }

    }
    public class SchedulesLines
    {
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? tutor { get; set; }
        public string? Stat { get; set; }

        public string? subjects { get; set; }
        public int? sMin { get; set; }
        public int? Id { get; set; }
    }
    public class PayLines
    {
        
        public DateTime? PayDate { get; set; }
        public double? PayAmt { get; set; }
        public double? HST { get; set; }
        public string? Descs { get; set; }
        public int? Id { get; set; }
        public DateTime? dtPaid { get; set; }
        public double? PaidAmt { get; set; }
        public string? Stat { get; set; }
        public string? payMode { get; set; }
    }
    public class EmpLibrary
    {
        public dynamic? departs { get; set; }
        public List<Stations>? stations { get; set; }
        public List<Designations>? desig { get; set; }
        public List<JobStatus>? jobstatus { get; set; }
        public List<UserGroups>? usergroups { get; set; }
        public List<Locations>? loc { get; set; }
        public List<EHUB.Models.Administration.WorkLocation>? workLocations { get; set; }
        public List<int>? empLocationIds { get; set; }
        public List<StaffData>? linemgr { get; set; }
        public List<EmpEditLog>? empEditLog { get; set; }
        public List<StaffEdu>? staffedus { get; set; } 
        public List<SchedulesLines>? SchLines { get; set; }
        public List<StaffWorkHis>? workhist { get; set; }
        public List<StaffDocs>? staffdocs { get; set; }
        public List<Libs>? libs { get; set; }
        public Empolyee? emp { get; set; }
        public NewStudent? stu { get; set; }
        public TutoringSession? Sess { get; set; }
        public List<Grade>? grade { get; set; }
        public List<SelectListItem>? stuList { get; set; }
        public List<SelectListItem>? EmpList { get; set; }
        public List<SelectListItem>? SbjcList { get; set; }
    }
    public class StaffEdu()
    {
        public int empID { get; set; }
        public string qualification { get; set; }
        public string institute { get; set; }
        public string? description { get; set; } = "";
        public int? id { get; set; }
    }
    public class StaffDocs()
    {
        public int empID { get; set; }
        public string title { get; set; }
        public string type { get; set; }
        public string? docname { get; set; }
        public int? id { get; set; }
        [NotMapped]
        public IFormFile? doc { set; get; } = null;
    }
    public class DeactivateInfo()
    {
        public int empID { get; set; }
        public string? stat { get; set; }
        public string? descs { get; set; }
        public DateTime? tdate { get; set; }
        public DateTime? rdate { get; set; }
    }
    public class StaffWorkHis
    {
        public int empID { get; set; }
        public string organisation { get; set; }
        public string jobfield { get; set; }
        public string? description { get; set; } = "";
        public string? durationstart { get; set; }
        public string? durationend { get; set; }
        public string? duration { get; set; }
        public int? id { get; set; }
    }
    public class lstStations
    {
        public int StationID { get; set; }
        public string? Station { get; set; }
    }
    public class GeneralRqst
    {
        public List<lstStations>? lstStation { get; set; }
        public List<GeneralRequests>? generalrequest { get; set; }
        public List<ddEmps>? ddEmplist { get; set; }
        public List<EmpsList>? Emplist { get; set; }
        public List<Designations>? designations { get; set; }
        public List<ExcptRequests>? excptRequests { get; set; }
		public List<LeaveRequests>? leaveRequests { get; set; }
		public List<LeaveRequests>? rwRequests { get; set; }
	}
    public class GeneralRequests
    {
        public int id { get; set; }
        public string? aprvstatus { get; set; }
        public DateTime dtrequest { get; set; }
        public string RqType { get; set; }
        public string frmSt { get; set; }
        public string toSt { get; set; }
        public DateTime? dtaprv { get; set; } = DateTime.MinValue;
        public string? RqDesc { get; set; } = null;
        public string? rqstfor { get; set; } = null;
        public string? rqstby { get; set; } = null;
        public string? aprvby { get; set; } = null;
        public DateTime? Created { get; set; } = null;
    }
    public class ddEmps
    {
        public int EmpID { get; set; }
        public int EmpCode { get; set; }
        public string FName { get; set; }
        public string? LName { get; set; }
        public int? Designation { get; set; }
        public int? RManager { get; set; }

    }
    public class EmpsList
    {
        public int EmpID { get; set; }
        public int EmpCode { get; set; }
        public string FName { get; set; }
        public string? LName { get; set; }
        public int? Designation { get; set; }
        public int? RManager { get; set; }
        public int? Station { get; set; }
    }
    public class TransferForm
    {
        public DateTime dtrequest { get; set; }
        public int EmpID { get; set; }
        public int FrmId { get; set; }
        public int ToId { get; set; }
        public string? RqDesc { get; set; } = null;
    }
    public class ExcptRequests
    {
        public DateTime? dtExem { get; set; }
        public string? ExemType { get; set; }
        public string? Remarks { get; set; }
        public string? FlagType { get; set; }
        public string? aprvstatus { get; set; }
        public int? EmpCode { get; set; }
        public string? FName { get; set; }
        public string? LName { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? dtaprv { get; set; }
        public int? aprvcode { get; set; }
        public string? aprvfname { get; set; }
        public string? aprvlname { get; set; }
        public int? Id { get; set; }
    }
}
