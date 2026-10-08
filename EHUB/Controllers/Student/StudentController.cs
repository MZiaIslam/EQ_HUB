using EHUB.Models.Administration;
using EHUB.Models.Home;
using EHUB.Models.HRManagement;
using EHUB.Models.Leaves;
using EHUB.Models.Login;
using EHUB.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using NuGet.Configuration;
using NuGet.Protocol.Plugins;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
namespace EHUB.Controllers.HRManagement
{
    [AuthUser]
    public class StudentController : BaseController
    {
        private readonly DataContext _dContext;
        private readonly IWebHostEnvironment _wHostEnv;
        GM gM = new GM();
        public StudentController(DataContext dContext, IWebHostEnvironment wHostEnv, GM gm)
            : base(dContext)
        {
            _dContext = dContext;
            _wHostEnv = wHostEnv;
        }
        [AuthWrite]
        public ActionResult StaffDataReport()
        {
            return View();
        }
        public IActionResult Index()
        {
            return RedirectToAction("index", "home");
        }
        [HttpPost]
        public IActionResult StudentRecord(Fillterdata fillterdata)
        {
            var _studentdata = new List<Student>();
            if (ModelState.IsValid)
            {
                _studentdata = _dContext.Database.SqlQuery<Student>($"usp_StudentRecord").ToList();
            }
            if (fillterdata.empstat != "0")
            {
                _studentdata = _studentdata.Where(x => x.Stat == fillterdata.empstat).ToList();
            }
            if (fillterdata.emp != "0")
            {
                _studentdata = _studentdata.Where(x => x.StudentID.ToString() == fillterdata.emp).ToList();
            }
            if (fillterdata.chkdt == "on")
            {
                _studentdata = _studentdata.Where(x => x.AppDate >= fillterdata.fromdate && x.AppDate <= fillterdata.todate).ToList();
            }
            var fillters = new fillters();
            fillters.students = _studentdata;
            fillters.fills = fillterdata;
            return View(fillters);
        }
        public IActionResult StudentRecord()
        {
            var _student = _dContext.Database.SqlQuery<Student>($"usp_StudentRecord").AsEnumerable().Select(p => new SelectListItem
            {
                Value = p.StudentID.ToString(),
                Text = p.StudentName.ToString()
            }).ToList();
            var _studentdata = new List<Student>();
            var fillters = new fillters();
            fillters.emps = _student;
            fillters.students = _studentdata;
            return View(fillters);
        }
        [HttpPost]
        [HttpGet]
        [AuthWrite]
        public async Task<ActionResult> StudentRegistrationAsync(NewStudent stud)
        {
            var studID = stud.StudentID;
            var chkEmail = stud.StudentID;
            if (ModelState.IsValid)
            {
                try
                {
                    var uinfo = User.Claims.ToArray();
                    var em = await _dContext.Database.SqlQueryRaw<EmpIds>
                            ($"usp_SaveStudent @StudentID,@StudentName,@Guardian,@CellNo,@Email,@Relationship,@DOB,@GradeID,@School,@TutoringFormat,@PreferredSchedule," +
                            $"@SessionsFrequency,@HealthConcerns,@EmergencyContact,@Subjects,@AcadPerformance,@LearningGoals,@Stat,@Gender",
                        new SqlParameter("@StudentID", stud.StudentID),
                            new SqlParameter("@StudentName", stud.StudentName),
        new SqlParameter("@Guardian", stud.Guardian),
        new SqlParameter("@CellNo", (object)stud.CellNo ?? DBNull.Value),
        new SqlParameter("@Email", (object)stud.Email ?? DBNull.Value),
        new SqlParameter("@Relationship", (object)stud.Relationship ?? DBNull.Value),
        new SqlParameter("@DOB", (object)stud.DOB ?? DBNull.Value),
        new SqlParameter("@GradeID", stud.GradeID),
        new SqlParameter("@School", stud.School),
        new SqlParameter("@TutoringFormat", (object)stud.TutoringFormat ?? DBNull.Value),
        new SqlParameter("@PreferredSchedule", (object)stud.PreferredSchedule ?? DBNull.Value),
        new SqlParameter("@SessionsFrequency", (object)stud.SessionsFrequency ?? DBNull.Value),
        new SqlParameter("@Relationship", (object)stud.Relationship ?? DBNull.Value),
        new SqlParameter("@HealthConcerns", (object)stud.HealthConcerns ?? DBNull.Value),
        new SqlParameter("@EmergencyContact", (object)stud.EmergencyContact ?? DBNull.Value),
        new SqlParameter("@Subjects", (object)stud.Subjects ?? DBNull.Value),
        new SqlParameter("@UserID", uinfo[2].Value),
        new SqlParameter("@AcadPerformance", (object)stud.AcadPerformance ?? DBNull.Value),
        new SqlParameter("@Gender", (object)stud.Gender ?? DBNull.Value),
        new SqlParameter("@LearningGoals", (object)stud.LearningGoals ?? DBNull.Value),
        new SqlParameter("@Stat", stud.Stat)
        ).ToListAsync();
                    studID = em[0].EmpID;
                    stud.StudentID = studID;
                    if (stud.photo != null)
                    {
                        var path = Path.Combine(_wHostEnv.WebRootPath + "/dist/img/StudentData/pics/" + studID + ".png");
                        stud.photo.CopyTo(new FileStream(path, FileMode.Create));
                    }
                    if (stud.Stat == "Active" && chkEmail == 0)
                    {
                        string subject = $"Welcome to EQUATION HUB! 🎉";
                        string body = $"Dear {stud.Guardian},<br/><br/>We are delighted to welcome you and your family to EQUATION HUB.<br/>Your child, {stud.StudentName}, has been successfully enrolled, and we are excited to begin this journey together.<br/>At EQUATION HUB, we are committed to providing a safe, supportive, and inspiring environment where students can learn, grow, and achieve their full potential.<br/><br/><strong>Student Portal Access</strong><br>You can access the Equation Hub Student Portal using the following login details:<br><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" width=\"100%\" style=\"max-width:600px; border:1px solid #e0e0e0; border-radius:6px; margin:15px 0;\"> <tr> <td style=\"padding:12px 15px; background:#f5f7fa; border-bottom:1px solid #e0e0e0; width:35%;\"> <strong>Portal</strong> </td> <td style=\"padding:12px 15px; border-bottom:1px solid #e0e0e0;\"> <a href=\"https://equationhubportal.ca/Login\" style=\"color:#007bff; text-decoration:none;\"> https://equationhubportal.ca/ </a> </td> </tr> <tr> <td style=\"padding:12px 15px; background:#f5f7fa; border-bottom:1px solid #e0e0e0;\"> <strong>Login ID</strong> </td> <td style=\"padding:12px 15px; border-bottom:1px solid #e0e0e0;\"> {stud.Email} </td> </tr> <tr> <td style=\"padding:12px 15px; background:#f5f7fa;\"> <strong>Temporary Password</strong> </td> <td style=\"padding:12px 15px;\"> <strong>Pass@ehub1</strong> </td> </tr> </table><br>The password provided above is a <strong>temporary password</strong> and should be changed immediately after your first login.<br><br>If you have any questions, please don’t hesitate to reach out to us.<br/>We look forward to working closely with you in supporting {stud.StudentName}’s academic journey.<br/><br/>Warm regards,<br/><img src='https://equationhubportal.ca/dist/img/eqhub.png' alt='eqhub' style='width: 100px;'><br/><b>Equation Hub</b><br/>2 Orchard Heights Boulevard<br/>Aurora, Ontario L4G 6T5, Canada<br/>(905) 409-6284<br/>www.equationhub.ca";
                        gM.SendEmail(stud.Email, subject, body);
                    }
                }
                catch (Exception ex)
                {
                    TempData["msg"] = "There was an error. " + Environment.NewLine + ex.Message;
                }
            }
            var grade = _dContext.Database.SqlQuery<Grade>($"SELECT GradeID, GradeLevel FROM tblGradeLevels").ToList();
            EmpLibrary empLibrary = new EmpLibrary();
            empLibrary.stu = stud;
            empLibrary.grade = grade;
            return View(empLibrary);
        }
        [HttpPost]
        [HttpGet]
        [AuthWrite]
        public async Task<ActionResult> EditStudentAsync(NewStudent stud, int id)
        {
            var StudentID = id;
            if (stud == null || stud.StudentID == 0)
            {
                stud = _dContext.Database.SqlQueryRaw<NewStudent>($"select StudentID,StudentName,Guardian,CellNo,Email,Relationship,DOB,GradeID,School,TutoringFormat,PreferredSchedule,SessionsFrequency,HealthConcerns,EmergencyContact,Subjects,AcadPerformance,LearningGoals,Stat,Gender,AppDate from tblStudents where StudentID=@StudentID",
                new SqlParameter("@StudentID", StudentID)).First();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var uinfo = User.Claims.ToArray();
                    var em = await _dContext.Database.SqlQueryRaw<EmpIds>
                            ($"usp_SaveStudent @StudentID,@StudentName,@Guardian,@CellNo,@Email,@Relationship,@DOB,@GradeID,@School,@TutoringFormat,@PreferredSchedule," +
                            $"@SessionsFrequency,@HealthConcerns,@EmergencyContact,@Subjects,@AcadPerformance,@LearningGoals,@Stat,@Gender",
                        new SqlParameter("@StudentID", stud.StudentID),
                            new SqlParameter("@StudentName", stud.StudentName),
        new SqlParameter("@Guardian", stud.Guardian),
        new SqlParameter("@CellNo", (object)stud.CellNo ?? DBNull.Value),
        new SqlParameter("@Email", (object)stud.Email ?? DBNull.Value),
        new SqlParameter("@Relationship", (object)stud.Relationship ?? DBNull.Value),
        new SqlParameter("@DOB", (object)stud.DOB ?? DBNull.Value),
        new SqlParameter("@GradeID", stud.GradeID),
        new SqlParameter("@School", stud.School),
        new SqlParameter("@TutoringFormat", (object)stud.TutoringFormat ?? DBNull.Value),
        new SqlParameter("@PreferredSchedule", (object)stud.PreferredSchedule ?? DBNull.Value),
        new SqlParameter("@SessionsFrequency", (object)stud.SessionsFrequency ?? DBNull.Value),
        new SqlParameter("@Relationship", (object)stud.Relationship ?? DBNull.Value),
        new SqlParameter("@HealthConcerns", (object)stud.HealthConcerns ?? DBNull.Value),
        new SqlParameter("@EmergencyContact", (object)stud.EmergencyContact ?? DBNull.Value),
        new SqlParameter("@Subjects", (object)stud.Subjects ?? DBNull.Value),
        new SqlParameter("@UserID", uinfo[2].Value),
        new SqlParameter("@AcadPerformance", (object)stud.AcadPerformance ?? DBNull.Value),
        new SqlParameter("@Gender", (object)stud.Gender ?? DBNull.Value),
        new SqlParameter("@LearningGoals", (object)stud.LearningGoals ?? DBNull.Value),
        new SqlParameter("@Stat", (object)stud.Stat ?? 3)
        ).ToListAsync();
                    StudentID = em[0].EmpID;
                    stud.StudentID = StudentID;
                    if (stud.photo != null)
                    {
                        var path = Path.Combine(_wHostEnv.WebRootPath + "/dist/img/StudentData/pics/" + StudentID + ".png");
                        stud.photo.CopyTo(new FileStream(path, FileMode.Create));
                    }
                }
                catch (Exception ex)
                {
                    TempData["msg"] = "There was an error. " + Environment.NewLine + ex.Message;
                }
            }
            var grade = _dContext.Database.SqlQuery<Grade>($"SELECT GradeID, GradeLevel FROM tblGradeLevels").ToList();
            EmpLibrary empLibrary = new EmpLibrary();
            empLibrary.stu = stud;
            empLibrary.grade = grade;
            return View(empLibrary);
        }
        [HttpPost]
        public IActionResult Lists([FromBody] Dictionary<string, object> jsonData)
        {
            string s = jsonData["s"].ToString();
            string js = "{\"[Sub-Department]\":" + gM.Data2Json("SELECT Depart val,Depart txt FROM tblDepartments WHERE (ParentId IS NOT NULL)") + ",\"Designation\":" + gM.Data2Json("SELECT Designation val,Designation txt FROM tblDesignations") + ",\"EmpStatus\":[{\"val\":1,\"txt\":\"Active\"},{\"val\":0,\"txt\":\"Inactive\"}] }";
            return Ok(js);
        }
        [HttpPost]
        public ActionResult SetSPass(string pass, string rpass, string empid)
        {
            if (IsPasswordStrong(pass))
            {
                if (pass == rpass && pass != "undefined" && pass != " " && pass != null && pass != "")
                {
                    var uinfo = User.Claims.ToArray();
                    var epass = gM.HashPasword(pass);
                    var em = _dContext.Database.SqlQueryRaw<EmpIds>($"UPDATE tblStudents SET Password = @pass WHERE (StudentID = @StudentID)",
                        new SqlParameter("@StudentID", empid),
                        new SqlParameter("@pass", epass)).ToList();
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
    }
}
