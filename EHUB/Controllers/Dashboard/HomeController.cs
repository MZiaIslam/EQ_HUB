using EHUB.Models;
using EHUB.Models.Administration;
using EHUB.Models.Home;
using EHUB.Models.HRManagement;
using EHUB.Models.Leaves;
using EHUB.Models.Login;
using EHUB.Models.Tickets;
using EHUB.Services;
using EHUB.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace EHUB.Controllers.Dashboard
{
    [AuthUser]
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DataContext _dContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        GM gM = new GM();
        AuthRights authRights = new AuthRights();
        public HomeController(DataContext dContext, ILogger<HomeController> logger,  IHttpContextAccessor httpContextAccessor)
            : base(dContext)
        {
            _logger = logger;
            _dContext = dContext;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<IActionResult> Index()
        {
            var uinfo = User.Claims.ToArray();
            var chk = authRights.chk();
            var mgr = authRights.mgr();
          await  CheckAndSendDailyRemindersAsync();
             //var notifications = _dContext.Database.SqlQueryRaw<LowSessionNotification>("usp_LowSessionNotifications").ToList();
             //ViewBag.LowSessionNotifications = notifications;
             ViewData["pgHeader"] = Greetings() + " 🎉";
            var _emps = _dContext.Database.SqlQuery<StaffData>($"usp_StaffRecord").AsEnumerable().Where(p => p.empstatus == 1).Select(p => new SelectListItem
            {
                Value = p.empid.ToString(),
                Text = p.empname,
            }).ToList();
            var dash = new Dash
            {
                emps = _emps,
            };
            return View(dash);
        }
        [HttpPost]
        public async Task<ActionResult> AttCalanderAsync()
        {
            IList<AttCalender> edu = null;
            if (ModelState.IsValid)
            {
                var uinfo = User.Claims.ToArray();
                edu = await _dContext.Database.SqlQueryRaw<AttCalender>
                    ($"usp_DashboardCalander @month,@userid",
                new SqlParameter("@month", DateTime.Now.ToString("MMM yyyy")),
                new SqlParameter("@userid", uinfo[2].Value)).ToListAsync();
            }
            return Json(edu);
        }
        [HttpPost]
        public async Task<ActionResult> AttMonthly(int empid, string month)
        {
            var uinfo = User.Claims.ToArray();
            empid = empid == 0 ? int.Parse(uinfo[2].Value) : empid;
            IList<AttChart> attchart = null;
            IList<AttSumm> attsumm = null;
            if (ModelState.IsValid)
            {
                attchart = await _dContext.Database.SqlQueryRaw<AttChart>
                    ($"usp_MonthlyAttendance @month,@EmpID",
                new SqlParameter("@month", month),
                new SqlParameter("@EmpID", empid)).ToListAsync();
                attsumm = await _dContext.Database.SqlQueryRaw<AttSumm>
              ($"usp_AttenSumm @month,@EmpID",
          new SqlParameter("@month", month),
          new SqlParameter("@EmpID", empid)).ToListAsync();
            }
            var grpLeave = attchart.GroupBy(p => p.stat).Select(g => new
            {
                cat = g.Key,
                count = g.Count(),
                name = g.FirstOrDefault(u => u.stat == g.Key).color
            });
            var series = (from d in grpLeave
                          select new
                          {
                              name = d.cat,
                              color = d.name,
                              data = (from s in attchart
                                      select new
                                      {
                                          y = (((d.cat == "Late" || d.cat == "Early" || d.cat == "Present" || d.cat == "Halfday" || d.cat == "Shortday") && s.stat == d.cat) ? (s.whours == null ? 2 : (s.whours == 0 ? .05 : s.whours)) : (s.stat == d.cat ? 2 : 0)),
                                          timein = s.timein,
                                          outtime = s.outtime,
                                          thours = s.thours,
                                          lvstat = s.lvstat
                                      }).ToList()
                          }).ToList();
            string[] categories = attchart.OfType<object>().Select(o => DateTime.Parse((((EHUB.Models.Home.AttChart)o).dtatt).ToString()).ToString("dd MMM")).ToArray();
            string tb = gM.HTMLchTable("usp_AttenCP '" + month + "'," + empid, "tb1", "table");
            string tb2 = gM.HTMLchTable("usp_AttenLateINOUT '" + month + "'," + empid, "tb2", "table");
            var data = new List<object>();
            data.Add(series);
            data.Add(categories);
            data.Add(attsumm);
            data.Add(tb);
            data.Add(tb2);
            return Json(data);
        }
        [HttpPost]
        public async Task<ActionResult> AttOverall(string dt)
        {
            var uinfo = User.Claims.ToArray();
            var chk = authRights.chk();
            var mgr = authRights.mgr();
            var mgrid = 0;
            if (mgr != 0 && chk != "f")
            {
                mgrid = int.Parse(uinfo[2].Value);
            }
            var overall = await _dContext.Database.SqlQueryRaw<OverallAtt>
                 ($"usp_overallAtt @dt,@linemgr",
             new SqlParameter("@dt", dt),
             new SqlParameter("@linemgr", mgrid)).ToListAsync();
            return Json(overall);
        }
        [HttpPost]
        public async Task<ActionResult> lstAbsents(string dt)
        {
            var uinfo = User.Claims.ToArray();
            var _staffdata = await _dContext.Database.SqlQueryRaw<StaffData>($"usp_AbsentStaff @dt",
            new SqlParameter("@dt", dt)).ToListAsync();
            var chk = authRights.chk();
            var mgr = authRights.mgr();
            if (mgr != 0 && chk != "f")
            {
                _staffdata = _staffdata.Where(x => x.RManager.ToString() == uinfo[2].Value).ToList();
            }
            return Json(_staffdata);
        }
        [HttpPost]
        public async Task<ActionResult> lstLeaves(string dt)
        {
            var uinfo = User.Claims.ToArray();
            var _staffdata = await _dContext.Database.SqlQueryRaw<StaffData>($"usp_OnLeaveStaff @dt",
            new SqlParameter("@dt", dt)).ToListAsync();
            var chk = authRights.chk();
            var mgr = authRights.mgr();
            if (mgr != 0 && chk != "f")
            {
                _staffdata = _staffdata.Where(x => x.RManager.ToString() == uinfo[2].Value).ToList();
            }
            return Json(_staffdata);
        }
        public IActionResult rptDash()
        {
            var uinfo = User.Claims.ToArray();
            try
            {
                if (uinfo[3].Value == "18")
                {
                    return Ok($"SELECT top(10) StudentName [Student Name],format(PayDate,'dd MMMM yyyy') [Invoice Date], format(PayAmt+HST,'#.00') AS Payables, (PaidAmt) AS Paid, (PayAmt+HST - PaidAmt) AS Balance FROM tblStudentPayments INNER JOIN tblStudents ON tblStudentPayments.StudentID = tblStudents.StudentID where Email = '{uinfo[1].Value}' order by PayDate desc --|||INVOICES|0||1|0| ||1015|0|250|col-md-6");
                }
                else
                {
                    string rptid = gM.FillDSet("SELECT RptID FROM tblReportHDR WHERE (Dashboard = 1) AND (UserID = " + uinfo[2].Value + ")").Tables[0].Rows[0][0].ToString();
                    string res = gM.FillDSet("SELECT dbo.fn_RptDTLs('" + rptid + "')").Tables[0].Rows[0][0].ToString();
                    return Ok(res);
                }
            }
            catch (Exception)
            {
                return Ok("");
            }
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public static string Greetings()
        {
            DateTime currentTime = DateTime.Now;
            int getHours = currentTime.Hour;
            string stringReturn = "";
            if (getHours >= 5 && getHours < 12)
            {
                stringReturn = "Good Morning";
            }
            else if (getHours >= 12 && getHours < 17)
            {
                stringReturn = "Good Afternoon";
            }
            else if (getHours >= 17 && getHours <= 24 || getHours < 5)
            {
                stringReturn = "Good Evening";
            }
            else
            {
                return null;
            }
            return stringReturn;
        }
        [HttpGet]
        public ActionResult clock(string id)
        {
            var uinfo = User.Claims.ToArray();
            //        _httpContextAccessor.HttpContext.Session.SetString("clock", id);
            //        if (uinfo[6].Value == "1")
            //        {
            //            var _leaves = _dContext.Database.SqlQueryRaw<LvIds>($"usp_HWAttendance @EmpId",
            //new SqlParameter("@EmpID", uinfo[2].Value)).ToList();
            //        }
            return Ok("");
        }
        [HttpPost]
        public ActionResult ChangePass(string pass, string rpass)
        {
            if (IsPasswordStrong(pass))
            {
                if (pass == rpass && pass != "undefined" && pass != " " && pass != null && pass != "")
                {
                    var epass = gM.HashPasword(pass);
                    var uinfo = User.Claims.ToArray();
                    if (uinfo[3].Value=="18")
                    {
                        var em = _dContext.Database.SqlQueryRaw<EmpIds>($"UPDATE tblStudents SET Password = @pass WHERE (StudentID = @StudentID)",
                          new SqlParameter("@StudentID", uinfo[2].Value),
                          new SqlParameter("@pass", epass)).ToList();
                    }
                    else
                    {
                        var em = _dContext.Database.SqlQueryRaw<EmpIds>($"UPDATE tblEmployees SET Password = @pass WHERE (EmpID = @EmpID)",
                          new SqlParameter("@EmpID", uinfo[2].Value),
                          new SqlParameter("@pass", epass)).ToList();
                    }
                      
                }
            }
            else
            {
                Console.WriteLine("Password is not strong enough.");
            }
            return Ok("");
        }
        public static bool IsPasswordStrong(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return false;
            }
            // Minimum length of 8 characters
            if (password.Length < 8)
            {
                return false;
            }
            // Must contain at least one uppercase letter
            if (!Regex.IsMatch(password, @"[A-Z]"))
            {
                return false;
            }
            // Must contain at least one lowercase letter
            if (!Regex.IsMatch(password, @"[a-z]"))
            {
                return false;
            }
            // Must contain at least one digit
            if (!Regex.IsMatch(password, @"\d"))
            {
                return false;
            }
            // Must contain at least one special character
            if (!Regex.IsMatch(password, @"[\W_]+"))
            {
                return false;
            }
            // If all checks pass, return true for strong password
            return true;
        }
        [HttpPost]
        [HttpGet]
        public async Task<ActionResult> FeedbackAsync(fback feedback, int id, int msgid, bool emailed)
        {
            SessionSchedules sessionSchedules = _dContext.Database.SqlQueryRaw<SessionSchedules>($"select StudentName,Guardian,Relationship,subjects,tutor,Stat,StartTime,EndTime,id,Email,empMail,points,TutorID from vwSchedules where id=@id",
            new SqlParameter("@id", id)).First();
            if (ModelState.IsValid)
            {
                try
                {
                    string fileName = null;
                    var fileurl = "";
                    if (feedback.Attachment != null && feedback.Attachment.Length > 0)
                    {
                        var folder = DateTime.Now.ToString("yyyMMddhhmmss");
                        var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", folder);
                        Directory.CreateDirectory(uploads); // ensure path exists
                        fileName = Path.GetFileName(feedback.Attachment.FileName);
                        fileurl = Path.Combine(folder, fileName);
                        var filePath = Path.Combine(uploads, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await feedback.Attachment.CopyToAsync(stream);
                        }
                    }
                    var uinfo = User.Claims.ToArray();
                    var _docs = await _dContext.Database.SqlQueryRaw<EmpIds>
                  ($"usp_AddFeedback @id,@details,@createdby,@utype,@filepath,@msgid",
              new SqlParameter("@id", id),
              new SqlParameter("@filepath", fileurl),
              new SqlParameter("@msgid", msgid),
              new SqlParameter("@createdby", uinfo[2].Value),
               new SqlParameter("@utype", (uinfo[3].Value == "18" ? "s" : "e")),
              new SqlParameter("@details", feedback.details)).ToListAsync();
                    var email = uinfo[3].Value == "18" ? sessionSchedules.empMail : sessionSchedules.Email;
                    var hdr = $"<table width=\"100%\"><tbody><tr>  <td style=\"vertical-align:top; width:45%;\"><strong>Student Name</strong><br><div>{sessionSchedules.StudentName}</div>  </td>  <td style=\"vertical-align:top; width:55%;\"><strong>Guardian Name</strong><br><div>{sessionSchedules.Guardian} ({sessionSchedules.Relationship})</div>  </td></tr><tr>  <td style=\"padding-top:12px;\"><strong>Subject(s)</strong><br><div>{sessionSchedules.Subjects}</div>  </td>  <td style=\"padding-top:12px;\"><strong>Tutor</strong><br><div>{sessionSchedules.Tutor}</div>  </td></tr><tr>  <td colspan=\"2\" style=\"padding-top:12px;\"><strong>Date &amp; Timings</strong><br><div>  {sessionSchedules.StartTime.ToString("ddd, MMM dd, yyyy ")} &nbsp;&nbsp; {sessionSchedules.StartTime.ToString("hh:mm tt")} - {sessionSchedules.EndTime.ToString("hh:mm tt")}</div>  </td></tr></tbody></table><br/>";
                    if (emailed)
                    {
                        gM.SendEmail(email, "Feedback - EQUATION HUB", (hdr + feedback.details), feedback.Attachment);
                    }
                }
                catch (Exception ex)
                {
                    TempData["msg"] = "There was an error. " + Environment.NewLine + ex.Message;
                }
            }
            var fbLines = _dContext.Database.SqlQuery<feedbacks>($"usp_feedbacks {id}").ToList();
            var _emps = _dContext.Database.SqlQuery<StaffData>($"usp_StaffRecord").AsEnumerable().Where(p => p.empstatus == 1).Select(p => new SelectListItem
            {
                Value = p.empid.ToString(),
                Text = p.empname,
            }).ToList();
            var bag = new bag();
            bag.feedbacks = fbLines;
            bag.SessionSch = sessionSchedules;
            bag.emps = _emps;
            bag.SelectedEmpId = sessionSchedules.TutorID;
            return View(bag);
        }
        public async Task<ActionResult> SetStat(string stat, string points, string id, string stime, string etime, string tutor)
        {
            var uinfo = User.Claims.ToArray();
            if ((uinfo[3].Value == "1"|| uinfo[3].Value == "20") && tutor != null)
            {
                gM.FillDSet($"UPDATE tblSessionSchedule SET Stat = '{stat}',points={points},StartTime = '{stime.Replace("T", " ")}', EndTime = '{etime.Replace("T", " ")}', TutorID ={tutor}, updateby = {uinfo[2].Value}, updatedt = GETDATE() WHERE  (Id = {id})");
            }
            else
            {
                gM.FillDSet($"UPDATE tblSessionSchedule SET Stat = '{stat}',points={points}, updateby = {uinfo[2].Value}, updatedt = GETDATE() WHERE  (Id = {id})");
            }
            return Json("");
        }
        [HttpGet]
        public async Task<ActionResult> DelMessageAsync(string id)
        {
            if (ModelState.IsValid)
            {
                gM.FillDSet($"UPDATE tblFeedback SET isDel = 1 WHERE (Id = {id})");
            }
            return Json("");
        }
        public IActionResult getHours(string month)
        {
            var uinfo = User.Claims.ToArray();
            try
            {
                string res;
                if (uinfo[3].Value == "19")
                {
                    res = gM.HTMLchTable($"exec usp_getHours '{month}',{uinfo[2].Value}", "h", "table table-sm table-hover table-striped tb-css");
                }
                else
                {
                    res = gM.HTMLchTable($"exec usp_getHoursStudent '{month}','{uinfo[1].Value}'", "h", "table table-sm table-hover table-striped tb-css");
                }
                return Ok(res);
            }
            catch (Exception)
            {
                return Ok("");
            }
        }
        public IActionResult ScheduledSessions(string month, string emp)
        {
            var uinfo = User.Claims.ToArray();
            try
            {
                string res;
                res = gM.Data2Json("usp_ScheduledSessions " + uinfo[3].Value + "," + uinfo[2].Value + ",'" + uinfo[1].Value + "','" + month + "'," + emp);
                return Ok(res);
            }
            catch (Exception)
            {
                return Ok("");
            }
        }
        public IActionResult getInvoices(string month)
        {
            var uinfo = User.Claims.ToArray();
            try
            {
                string res;
                res = gM.HTMLchTable("usp_getInvoices " + uinfo[3].Value + ",'" + month + "'", "tblInvoices", "table table-sm dataTable table-hover table-striped tb-css");
                return Ok(res);
            }
            catch (Exception)
            {
                return Ok("");
            }
        }
     
    }

}
