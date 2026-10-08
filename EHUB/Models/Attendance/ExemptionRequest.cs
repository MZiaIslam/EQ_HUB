using EHUB.Models.Administration;
using EHUB.Models.HRManagement;
using EHUB.Models.Leaves;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
namespace EHUB.Models.Attendance
{
    public class ExemptRequest
    {
        public List<StaffData> staff { get; set; }
        public List<ExemptRqst> exemptRqst { get; set; }
    }
    public class ExemptData
    {
        public int EmpId { get; set; }
        public DateTime dtExem { get; set; }
        public string FlagType { get; set; }
        public int ExemType { get; set; }
        public string? Remarks { get; set; } = null;
    }
    public class AttendAmendment
    {
        public List<StaffData> staff { get; set; }
        public List<Departments> depart { get; set; }
        public List<Departments> sbdepart { get; set; }
        public List<ShiftList>? shifts { get; set; }
    }
    public class ShiftData
    {
        public int EmpCode { get; set; }
        public string fname { get; set; }
        public string Depart { get; set; }
        public int shiftId { get; set; }
    }
    public class AmendmentData
    {
        public int EmpCode { get; set; }
        public string fname { get; set; }
        public DateTime AttDate { get; set; }
        public string intime { get; set; }
        public string shifttime { get; set; }
        public string outtime { get; set; }
        public string Depart { get; set; }
        public int shiftId { get; set; }
    }
    public class ManualAttand
    {
        public int EmpCode { get; set; }
        public string? fname { get; set; }

        public string? intime { get; set; }
        public string? shiftName { get; set; }
        public string? outtime { get; set; }
        public string? Depart { get; set; }
      
    }
    public class ExemptRqst
    {
        public DateTime dtExem { get; set; }
        public string FlagType { get; set; }
        public string ExemType { get; set; }
        public string? aprvstatus { get; set; }
        public string? Remarks { get; set; } = null;
    }
    public class ExemptedDay
    {
        public int mode { get; set; }
        public string[]? EmpIds { get; set; }
        public string[]? DepartIds { get; set; }
        public DateTime dtExempt { get; set; }
        public int ExemTypeId { get; set; }
        public string Remks { get; set; }
        public string ExptMin { get; set; }
    }
    public class ExemptTypes
    {
        public int ExemTypeId { get; set; }
        public string? ExemType { get; set; }
    }
    public class ExemptInfo
    {
        public string dtExempt { get; set; }
        public string ExemType { get; set; }
        public int? ExptMin { get; set; }
        public string? Remks { get; set; }
        public string createdby { get; set; }
        public DateTime created { get; set; }
        public string? modifyby { get; set; } = null;
        public DateTime? modify { get; set; } = null;
        public int id { get; set; }
    }
    public class RWRequests
    {
        
             public string? Mgr { get; set; }
        public string? eName { get; set; }
        public int RWId { get; set; }
        public DateTime rDate { get; set; }
        public DateTime inTime { get; set; }
        public DateTime outTime { get; set; }
        public string reason { get; set; }
        public DateTime Created { get; set; }
        public string aprvstatus { get; set; }
        public DateTime? dtaprv { get; set; }
        public string? attachment { get; set; } = null;
    }
    public class RWRequest
    {
public int? RWId { get; set; }
        public int EmpId { get; set; }
        public DateTime rDate { get; set; }
        public TimeOnly inTime { get; set; }
        public TimeOnly outTime { get; set; }
        public string reason { get; set; }
        [NotMapped]
        public IFormFile? doc { set; get; } = null;


    }
}
