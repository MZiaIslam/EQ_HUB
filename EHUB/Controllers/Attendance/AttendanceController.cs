using EHUB.Models.Administration;
using EHUB.Models.Attendance;
using EHUB.Models.HRManagement;
using EHUB.Models.Leaves;
using EHUB.Models.Login;
using EHUB.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using NuGet.Configuration;
using NuGet.Protocol.Plugins;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection.Emit;
using System.Xml;
namespace EHUB.Controllers.HRManagement
{
    [AuthUser]
    public class AttendanceController : Controller
    {
        private readonly DataContext _dContext;
        private readonly IWebHostEnvironment _wHostEnv;
        private readonly GM _gm;
        GM gM = new GM();
        public AttendanceController(DataContext dContext, IWebHostEnvironment wHostEnv, GM gm)
        {
            _dContext = dContext;
            _wHostEnv = wHostEnv;
            _gm = gm;
        }
        [AuthWrite]
        public ActionResult AttendanceSheet()
        {
            var uinfo = User.Claims.ToArray();
            AttendAmendment amendment = new AttendAmendment();
            var _staff = _dContext.Database.SqlQuery<StaffData>($"usp_StaffRecord").ToList();
            amendment.staff = _staff.Where(x => x.empstatus == 1).ToList();
            var _shift = _dContext.Database.SqlQuery<ShiftList>($"SELECT shiftId,shiftName FROM tblShifts WHERE  (isDel = 0)").ToList();
            var _dpart = _dContext.Database.SqlQuery<Departments>($"SELECT * FROM tblDepartments").ToList();
            amendment.depart = _dpart.Where(x => x.ParentId is null).ToList();
            amendment.sbdepart = _dpart.Where(x => x.ParentId is not null).ToList();
            amendment.shifts = _shift;
            return View(amendment);
        }
        [AuthWrite]
        public ActionResult AttendanceAnalyzer()
        {
            return View();
        }
        public ActionResult DutyRoster()
        {
            var uinfo = User.Claims.ToArray();
            IList<Departments> _Departments = null;
            _Departments = _dContext.Database.SqlQuery<Departments>($"SELECT * FROM tblDepartments order by Depart").ToList();
            var dd = (from d in _Departments
                      where d.ParentId == null
                      select new
                      {
                          DepartID = d.DepartID,
                          Depart = d.Depart,
                          SubDepartments = (from s in _Departments
                                            where s.ParentId == d.DepartID
                                            select new
                                            {
                                                DepartID = s.DepartID,
                                                Depart = s.Depart
                                            }).ToList()
                      }).ToList();
            pgRoster _pgroster = new pgRoster();
            _pgroster.departs = dd;
            IList<StaffData> _staff = null;
            _staff = _dContext.Database.SqlQuery<StaffData>($"usp_StaffRecord").ToList();
            _pgroster.staff = _staff.Where(x => x.empstatus == 1).ToList();
            return View(_pgroster);
        }
        [HttpPost]
        public async Task<ActionResult> DutyRoster(Roster roster)
        {
            var uinfo = User.Claims.ToArray();
            if (ModelState.IsValid)
            {
                try
                {
                    string Ids;
                    if (roster.mode == 1)
                    {
                        Ids = string.Join(",", roster.DepartIds);
                    }
                    else { Ids = string.Join(",", roster.EmpIds); }
                    var lv = await _dContext.Database.SqlQueryRaw<LvIds>
                            ($"usp_SaveRoster @Mode,@Ids,@fdate,@tdate,@intime,@outtime,@UserID",
                        new SqlParameter("@Mode", roster.mode),
                            new SqlParameter("@Ids", Ids),
                            new SqlParameter("@fdate", roster.fdate),
                            new SqlParameter("@tdate", roster.tdate),
                            new SqlParameter("@intime", roster.intime),
                            new SqlParameter("@outtime", roster.outtime),
                        new SqlParameter("@UserID", uinfo[2].Value)
                            ).ToListAsync();
                    var LvId = lv[0].LvId;
                }
                catch (Exception ex)
                {
                    TempData["msg"] = "There was an error. " + Environment.NewLine + ex.Message;
                }
            }
            return RedirectToAction("DutyRoster", "Attendance");
        }
        [HttpGet]
        public async Task<ActionResult> GetRoster(int id)
        {
            IList<RosterInfo> edu = null;
            if (ModelState.IsValid)
            {
                var uinfo = User.Claims.ToArray();
                edu = await _dContext.Database.SqlQueryRaw<RosterInfo>
                    ($"usp_RosterInfo @USERID",
                new SqlParameter("@USERID", id)).ToListAsync();
            }
            return Json(edu);
        }
        [HttpGet]
        public async Task<ActionResult> DelRosterAsync(string id)
        {
            IList<RosterInfo> edu = null;
            if (ModelState.IsValid)
            {
                var uinfo = User.Claims.ToArray();
                edu = await _dContext.Database.SqlQueryRaw<RosterInfo>
                    ($"usp_RosterDel @Id,@UserID",
                new SqlParameter("@UserID", uinfo[2].Value),
                new SqlParameter("@id", id)).ToListAsync();
            }
            return Json(edu);
        }
        [HttpPost]
        public IActionResult Lists([FromBody] Dictionary<string, object> jsonData)
        {
            string s = jsonData["s"].ToString();
            string js = "{\"[Sub-Department]\":" + gM.Data2Json("SELECT Depart val,Depart txt FROM tblDepartments WHERE (ParentId IS NOT NULL)") + ",\"Designation\":" + gM.Data2Json("SELECT Designation val,Designation txt FROM tblDesignations") + " }";
            return Ok(js);
        }
        [AuthWrite]
        public ActionResult ExemptionRequest()
        {
            var uinfo = User.Claims.ToArray();
            ExemptRequest exemptRequest = new ExemptRequest();
            var _exemptRqsts = _dContext.Database.SqlQueryRaw<ExemptRqst>($"usp_ExemptionRequests @EmpID",
                new SqlParameter("@EmpID", uinfo[2].Value)).ToList();
            var _staff = _dContext.Database.SqlQuery<StaffData>($"usp_StaffRecord").ToList();
            exemptRequest.staff = _staff.Where(x => x.empstatus == 1).ToList();
            exemptRequest.exemptRqst = _exemptRqsts;
            return View(exemptRequest);
        }
        [HttpPost]
        public async Task<ActionResult> ExemptionRequest(ExemptData exemptData)
        {
            var uinfo = User.Claims.ToArray();
            if (ModelState.IsValid)
            {
                try
                {
                    var expt = await _dContext.Database.SqlQueryRaw<LvIds>(
                        "usp_SaveExemption @EmpID,@dtExem,@ExemType,@Remarks,@FlagType,@CreatedBy",
                        new SqlParameter("@EmpID", uinfo[2].Value),
                        new SqlParameter("@CreatedBy", uinfo[2].Value),
                        new SqlParameter("@dtExem", exemptData.dtExem),
                        new SqlParameter("@ExemType", exemptData.ExemType),
                        new SqlParameter("@FlagType", exemptData.FlagType),
                        new SqlParameter("@Remarks", (object)exemptData.Remarks ?? DBNull.Value)
                    ).ToListAsync();
                }
                catch (Exception ex)
                {
                    TempData["msg"] = "There was an error: " + ex.Message;
                }
            }
            ExemptRequest exemptRequest = new ExemptRequest();
            var _staff = _dContext.Database.SqlQuery<StaffData>($"usp_StaffRecord").ToList();
            exemptRequest.staff = _staff.Where(x => x.empstatus == 1).ToList();
            var _exemptRqsts = _dContext.Database.SqlQuery<ExemptRqst>($"usp_ExemptionRequests {uinfo[2].Value}").ToList();
            exemptRequest.exemptRqst = _exemptRqsts;
            return View(exemptRequest);
        }
        public IActionResult Flags(DateTime dt, int staff)
        {
            var uinfo = User.Claims.ToArray();
            string flags = gM.Data2Json("usp_FlagTypes N'" + dt + "'," + uinfo[2].Value);
            return Json(flags);
        }
        public IActionResult ExemptDays()
        {
            var uinfo = User.Claims.ToArray();
            string exdays = gM.Data2Json("usp_ExemptDays " + uinfo[2].Value);
            return Json(exdays);
        }
        [AuthWrite]
        public ActionResult AttendanceAmendment()
        {
            var uinfo = User.Claims.ToArray();
            AttendAmendment amendment = new AttendAmendment();
            var _staff = _dContext.Database.SqlQuery<StaffData>($"usp_StaffRecord").ToList();
            amendment.staff = _staff.Where(x => x.empstatus == 1).ToList();
            var _shift = _dContext.Database.SqlQuery<ShiftList>($"SELECT shiftId,shiftName FROM tblShifts WHERE  (isDel = 0)").ToList();
            var _dpart = _dContext.Database.SqlQuery<Departments>($"SELECT * FROM tblDepartments").ToList();
            amendment.depart = _dpart.Where(x => x.ParentId is null).ToList();
            amendment.sbdepart = _dpart.Where(x => x.ParentId is not null).ToList();
            amendment.shifts = _shift;
            return View(amendment);
        }
        [AuthWrite]
        public ActionResult ManualAttendance()
        {
            var uinfo = User.Claims.ToArray();
            AttendAmendment amendment = new AttendAmendment();
            var _staff = _dContext.Database.SqlQuery<StaffData>($"usp_StaffRecord").ToList();
            amendment.staff = _staff.Where(x => x.empstatus == 1).ToList();
            var _shift = _dContext.Database.SqlQuery<ShiftList>($"SELECT shiftId,shiftName FROM tblShifts WHERE  (isDel = 0)").ToList();
            var _dpart = _dContext.Database.SqlQuery<Departments>($"SELECT * FROM tblDepartments").ToList();
            amendment.depart = _dpart.Where(x => x.ParentId is null).ToList();
            amendment.sbdepart = _dpart.Where(x => x.ParentId is not null).ToList();
            amendment.shifts = _shift;
            return View(amendment);
        }
        [HttpPost]
        public async Task<ActionResult> SetManualAttend(int EmpId, DateOnly dtAtt, TimeOnly inTime, TimeOnly outTime)
        {
            var uinfo = User.Claims.ToArray();
            if (ModelState.IsValid)
            {
                try
                {
                    var lv = await _dContext.Database.SqlQueryRaw<LvIds>(
                        "usp_SaveManualAttend @EmpID,@dtAtt,@inTime,@outTime",
                        new SqlParameter("@dtAtt", dtAtt),
                        new SqlParameter("@EmpID", EmpId),
                        new SqlParameter("@inTime", inTime),
                        new SqlParameter("@outTime", outTime)
                    ).ToListAsync();
                    var lvId = lv.FirstOrDefault()?.LvId;
                }
                catch (Exception ex)
                {
                    TempData["msg"] = "There was an error: " + ex.Message;
                }
            }
            return RedirectToAction("ManualAttendance", "Attendance");
        }
        [HttpGet]
        public async Task<ActionResult> ManualAttend(int empid, string dtAtt)
        {
            IList<ManualAttand> lst = null;
            if (ModelState.IsValid)
            {
                var uinfo = User.Claims.ToArray();
                lst = await _dContext.Database.SqlQueryRaw<ManualAttand>
                    ($"usp_getManualAttand @empId,@dtAtt",
                new SqlParameter("@empId", empid),
                new SqlParameter("@dtAtt", dtAtt)
                ).ToListAsync();
            }
            return Json(lst);
        }
        [HttpGet]
        public async Task<ActionResult> GetAmendment(int empid, string dtAtt, int depart, int sbdepart)
        {
            IList<AmendmentData> lst = null;
            if (ModelState.IsValid)
            {
                var uinfo = User.Claims.ToArray();
                lst = await _dContext.Database.SqlQueryRaw<AmendmentData>
                    ($"usp_getAmendAttand @dpart,@empcode,@sbdepart,@dtAtt",
                new SqlParameter("@dpart", depart),
                new SqlParameter("@empcode", empid),
                new SqlParameter("@sbdepart", sbdepart),
                new SqlParameter("@dtAtt", dtAtt)
                ).ToListAsync();
            }
            return Json(lst);
        }
        [HttpGet]
        public async Task<ActionResult> upTime(string itime, string otime, string date, int empcode, int shift)
        {
            var uinfo = User.Claims.ToArray();
            var _des = gM.FillDSet($"usp_EditAttand '" + itime + "','" + otime + "','" + date + "'," + empcode + "," + uinfo[2].Value + "," + shift).Tables[0].Rows;
            return Ok("");
        }
        [AuthWrite]
        public ActionResult ShiftAmendment()
        {
            var uinfo = User.Claims.ToArray();
            AttendAmendment amendment = new AttendAmendment();
            var _staff = _dContext.Database.SqlQuery<StaffData>($"usp_StaffRecord").ToList();
            amendment.staff = _staff.Where(x => x.empstatus == 1).ToList();
            var _dpart = _dContext.Database.SqlQuery<Departments>($"SELECT * FROM tblDepartments").ToList();
            var _shift = _dContext.Database.SqlQuery<ShiftList>($"SELECT shiftId,shiftName FROM tblShifts WHERE  (isDel = 0)").ToList();
            amendment.depart = _dpart.Where(x => x.ParentId is null).ToList();
            amendment.sbdepart = _dpart.Where(x => x.ParentId is not null).ToList();
            amendment.shifts = _shift;
            return View(amendment);
        }
        [HttpGet]
        public async Task<ActionResult> GetShiftAmendment(int empid, int depart, int sbdepart)
        {
            IList<ShiftData> lst = null;
            if (ModelState.IsValid)
            {
                var uinfo = User.Claims.ToArray();
                lst = await _dContext.Database.SqlQueryRaw<ShiftData>
                    ($"usp_getAmendShift @dpart,@empcode,@sbdepart",
                new SqlParameter("@dpart", depart),
                new SqlParameter("@empcode", empid),
                new SqlParameter("@sbdepart", sbdepart)
                ).ToListAsync();
            }
            return Json(lst);
        }
        [HttpGet]
        public async Task<ActionResult> upShift(string shiftId, int empcode)
        {
            var uinfo = User.Claims.ToArray();
            var _des = gM.FillDSet($"UPDATE tblEmployees SET shiftId = " + shiftId + " WHERE  (EmpCode = " + empcode + ");select 1 id").Tables[0].Rows;
            return Ok("");
        }
        [AuthWrite]
        public ActionResult RemoteWorkRequest()
        {
            var uinfo = User.Claims.ToArray();
            var rwRequests = _dContext.Database.SqlQueryRaw<RWRequests>("usp_RWRequests @EmpId",
                new SqlParameter("@EmpID", uinfo[2].Value)).ToList();
            return View(rwRequests);
        }
        [HttpPost]
        public async Task<ActionResult> RemoteWorkRequest(RWRequest rWRequest)
        {
            var uinfo = User.Claims.ToArray();
            if (ModelState.IsValid)
            {
                try
                {
                    var lv = await _dContext.Database.SqlQueryRaw<LvIds>(
                        "usp_SaveRWRequest @EmpID,@rDate,@inTime,@outTime,@reason,@attachment,@UserID,@RWId",
                        new SqlParameter("@rDate", rWRequest.rDate),
                        new SqlParameter("@EmpID", uinfo[2].Value),
                        new SqlParameter("@inTime", rWRequest.inTime),
                        new SqlParameter("@outTime", rWRequest.outTime),
                        new SqlParameter("@reason", (object)rWRequest.reason ?? DBNull.Value),
                        new SqlParameter("@attachment", rWRequest.doc?.FileName ?? (object)DBNull.Value),
                        new SqlParameter("@UserID", uinfo[2].Value),
                        new SqlParameter("@RWId", (object)rWRequest.RWId ?? DBNull.Value)
                    ).ToListAsync();
                    var lvId = lv.FirstOrDefault()?.LvId;
                    if (rWRequest.doc != null && lvId != null)
                    {
                        var directoryPath = Path.Combine(_wHostEnv.WebRootPath, "dist", "img", "StaffData", "RWRequest", lvId.ToString());
                        if (!Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(directoryPath);
                        }
                        var filePath = Path.Combine(directoryPath, rWRequest.doc.FileName);
                        using (var fileStream = new FileStream(filePath, FileMode.CreateNew))
                        {
                            await rWRequest.doc.CopyToAsync(fileStream);
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["msg"] = "There was an error: " + ex.Message;
                }
            }
            var rwRequests = _dContext.Database.SqlQueryRaw<RWRequests>("usp_RWRequests @EmpId",
                      new SqlParameter("@EmpID", uinfo[2].Value)).ToList();
            return View(rwRequests);
        }
        [AuthWrite]
        public ActionResult RWRequestApprovals()
        {
            var uinfo = User.Claims.ToArray();
            var _leaves = _dContext.Database.SqlQueryRaw<LeaveRequests>($"usp_RWPendingApprovals @EmpId",
                 new SqlParameter("@EmpID", uinfo[2].Value)).ToList();
            return View(_leaves);
        }
        [HttpGet]
        public ActionResult Refuse(int Id)
        {
            var uinfo = User.Claims.ToArray();
            var _leaves = _dContext.Database.SqlQueryRaw<LvIds>($"usp_RWRefuse @EmpId,@Id",
                 new SqlParameter("@EmpID", uinfo[2].Value),
                 new SqlParameter("@Id", Id)).ToList();
            return RedirectToAction("RWRequestApprovals", "Attendance");
        }
        [HttpGet]
        public ActionResult Approve(int Id)
        {
            var uinfo = User.Claims.ToArray();
            var _leaves = _dContext.Database.SqlQueryRaw<LvIds>($"usp_RWApprove @EmpId,@Id",
                new SqlParameter("@EmpID", uinfo[2].Value),
                new SqlParameter("@Id", Id)).ToList();
            return RedirectToAction("RWRequestApprovals", "Attendance");
        }
    }
}
