using EHUB.Models.Administration;
using EHUB.Models.Home;
using EHUB.Models.HRManagement;
using EHUB.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace EHUB.Controllers.HRManagement
{
	[AuthUser]
	public class SessionController : BaseController
	{
		private readonly DataContext _dContext;
		private readonly IWebHostEnvironment _wHostEnv;
		GM gM = new GM();
		public SessionController(DataContext dContext, IWebHostEnvironment wHostEnv, GM gm)
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
		public IActionResult ScheduledSessions(Fillterdata fillterdata)
		{
			var _studentdata = new List<Student>();
			if (ModelState.IsValid)
			{
				_studentdata = _dContext.Database.SqlQuery<Student>($"usp_SessionRecord {fillterdata.emp},{fillterdata.empstat},{fillterdata.fromdate ?? DateOnly.FromDateTime(DateTime.Now)},{fillterdata.todate ?? DateOnly.FromDateTime(DateTime.Now)},{fillterdata.chkdt ?? "off"}").ToList();
			}
			var _emps = _dContext.Database.SqlQuery<StaffData>($"usp_StaffRecord").AsEnumerable().Where(p => p.empstatus == 1).Select(p => new SelectListItem
			{
				Value = p.empid.ToString(),
				Text = p.empname,
				Selected = p.empid.ToString() == fillterdata.empstat ? true : false
			}).ToList();
			var _student = _dContext.Database.SqlQuery<Student>($"usp_StudentRecord").AsEnumerable().Where(p => p.Stat == "Active").Select(p => new SelectListItem
			{
				Value = p.StudentID.ToString(),
				Text = p.StudentName.ToString(),
				Selected = p.StudentID.ToString() == fillterdata.emp ? true : false
			}).ToList();
			var fillters = new fillters();
			fillters.students = _studentdata;
			fillters.fills = fillterdata;
			fillters.depart = _emps;
			fillters.emps = _student;
			return View(fillters);
		}
		[AuthWrite]
		public IActionResult ScheduledSessions()
		{
			var _student = _dContext.Database.SqlQuery<Student>($"usp_StudentRecord").AsEnumerable().Where(p => p.Stat == "Active").Select(p => new SelectListItem
			{
				Value = p.StudentID.ToString(),
				Text = p.StudentName.ToString()
			}).ToList();
			var _emps = _dContext.Database.SqlQuery<StaffData>($"usp_StaffRecord").AsEnumerable().Where(p => p.empstatus == 1).Select(p => new SelectListItem
			{
				Value = p.empid.ToString(),
				Text = p.empname
			}).ToList();
			var _studentdata = new List<Student>();
			var fillters = new fillters();
			fillters.emps = _student;
			fillters.depart = _emps;
			fillters.students = _studentdata;
			return View(fillters);
		}
		[HttpPost]
		[HttpGet]
		[AuthWrite]
		public async Task<ActionResult> CreateScheduleAsync(TutoringSession Sess, int StudentID)
		{
			if (Request.Method == HttpMethods.Get)
			{
				var activeSession = _dContext.Database
	.SqlQuery<int>($"""
		SELECT TOP 1 SessionID Value
		FROM tblSessions
		WHERE StudentID = {StudentID}
		ORDER BY SessionID DESC
		""")
	.FirstOrDefault();
				Sess.SessionId = activeSession == 0 ? null : activeSession;
			}
			var SessID = Sess.SessionId;
			if (Request.Method == HttpMethods.Post)
			{
				if (ModelState.IsValid)
				{
					try
					{
						var uinfo = User.Claims.ToArray();
						var em = await _dContext.Database.SqlQueryRaw<EmpIds>
								($"usp_SaveSession @StudentID,@GradeID,@PurchasedHrs,@TotalAmt,@StartTime,@EndTime,@Subjects,@TutorID,@SessionId,@UID,@Recurrence",
							new SqlParameter("@StudentID", Sess.StudentID),
								new SqlParameter("@GradeID", Sess.GradeID),
			new SqlParameter("@PurchasedHrs", (object)Sess.PurchasedHrs ?? DBNull.Value),
			new SqlParameter("@TotalAmt", (object)Sess.TotalAmt ?? DBNull.Value),
			new SqlParameter("@Recurrence", (object)Sess.Recurrence ?? DBNull.Value),
			new SqlParameter("@StartTime", (object)Sess.StartTime ?? DBNull.Value),
			new SqlParameter("@EndTime", (object)Sess.EndTime ?? DBNull.Value),
			new SqlParameter("@Subjects", string.Join(", ", Sess.Subjects)),
			new SqlParameter("@TutorID", Sess.TutorID),
			new SqlParameter("@SessionId", (object)Sess.SessionId ?? 0),
			new SqlParameter("@UID", uinfo[2].Value)
			).ToListAsync();
						SessID = em[0].EmpID;
						Sess.SessionId = SessID;
						if (Sess.Subjects.Any(subject => subject == 8))
						{
							DataTable stdInfo = gM.FillDSet($"SELECT StudentName, Guardian, Email FROM tblStudents WHERE (StudentID = {Sess.StudentID})").Tables[0];
							gM.SendEmail(stdInfo.Rows[0]["Email"]?.ToString(), $"Trial Session Scheduled for {stdInfo.Rows[0]["StudentName"]?.ToString()} - {Sess.StartTime:MMMM dd, yyyy}", $"Dear {stdInfo.Rows[0]["Guardian"]},<br><br>This is a reminder for the trial session scheduled for {stdInfo.Rows[0]["StudentName"]} on {Sess.StartTime:MMMM dd, yyyy} from {Sess.StartTime:hh:mm tt} to {Sess.EndTime:hh:mm tt}.<br>Please ensure that the student is prepared and available for the session.<br>Thank you for your attention.<br><br>Best regards,<br/><img src='https://equationhubportal.ca/dist/img/eqhub.png' alt='eqhub' style='width: 100px;'><br/><b>Equation Hub</b><br/>2 Orchard Heights Boulevard<br/>Aurora, Ontario L4G 6T5, Canada<br/>(905) 409-6284<br/>www.equationhub.ca", null);
						}
					}
					catch (Exception ex)
					{
						TempData["msg"] = "There was an error. " + Environment.NewLine + ex.Message;
					}
				}
			}
			var _student = _dContext.Database.SqlQuery<Student>($"usp_StudentRecord").AsEnumerable().Where(p => p.Stat == "Active").Select(p => new SelectListItem
			{
				Value = p.StudentID.ToString(),
				Text = p.StudentName.ToString(),
				Selected = p.StudentID.ToString() == Sess.StudentID.ToString() ? true : false
			}).ToList();
			var _emps = _dContext.Database.SqlQuery<StaffData>($"usp_StaffRecord").AsEnumerable().Where(p => p.empstatus == 1).Select(p => new SelectListItem
			{
				Value = p.empid.ToString(),
				Text = p.empname
			}).ToList();
			var _subjects = _dContext.Database.SqlQuery<Subjects>($"SELECT SubjectID, Subject FROM tblSubjects").AsEnumerable().Select(p => new SelectListItem
			{
				Value = p.SubjectID.ToString(),
				Text = p.Subject
			}).ToList();
			var grade = _dContext.Database.SqlQuery<Grade>($"SELECT GradeID, GradeLevel FROM tblGradeLevels").ToList();
			var SchLines = _dContext.Database.SqlQuery<SchedulesLines>($"usp_Schedules {SessID}").ToList();
			EmpLibrary empLibrary = new EmpLibrary();
			empLibrary.Sess = Sess;
			empLibrary.grade = grade;
			empLibrary.SchLines = SchLines;
			empLibrary.EmpList = _emps;
			empLibrary.SbjcList = _subjects;
			empLibrary.stuList = _student;
			return View(empLibrary);
		}
		[HttpPost]
		[HttpGet]
		[AuthWrite]
		public async Task<ActionResult> BillingPaymentsAsync(string StudentID, string PayDate, string PayAmt, string Descs, string PaidAmt, string dtPaid, string nID, string HST, string payMode, int id)
		{
			if (StudentID != null)
			{
				if (nID != "0")
				{
					gM.FillDSet($"Update tblStudentPayments set PayDate='{PayDate}', PayAmt='{PayAmt}', Descs='{Descs}', dtPaid='{dtPaid}', PaidAmt='{PaidAmt}',HST='{HST}',payMode='{payMode}' where ID={nID}");
				}
				else
				{
					nID = gM.FillDSet($"INSERT INTO tblStudentPayments (StudentID, PayDate, PayAmt, Descs,HST) VALUES ({StudentID},'{PayDate}','{PayAmt}','{Descs}','{HST}');SELECT SCOPE_IDENTITY() AS NewID").Tables[0].Rows[0][0].ToString();
				}
				string email = gM.FillDSet($"SELECT Email FROM tblStudents WHERE  (StudentID = {StudentID})").Tables[0].Rows[0][0].ToString();
				var reportParams = $"usp_PaymentInvoice {nID}";
				byte[] bytes = await Task.Run(() => gM.RunReport("DataSet1".Split('¦'), reportParams.Split('¦'), "EHUB.Invoice.rdlc", "pdf"));
				IFormFile formFile = gM.ConvertBytesToIFormFile(bytes, "invoice.pdf");
				gM.SendEmail(email, "INVOICE - EQUATION HUB", "Please find attached invoice.", formFile);
			}
			ViewBag.StudentID = id;
			StudentID = id.ToString();
			var _student = _dContext.Database.SqlQuery<Student>($"usp_StudentRecord").AsEnumerable().Where(p => p.Stat == "Active").Select(p => new SelectListItem
			{
				Value = p.StudentID.ToString(),
				Text = p.StudentName.ToString(),
				Selected = p.StudentID.ToString() == StudentID ? true : false
			}).ToList();
			EmpLibrary empLibrary = new EmpLibrary();
			empLibrary.stuList = _student;
			return View(empLibrary);
		}
		[HttpPost]
		[HttpGet]
		[AuthWrite]
		public async Task<ActionResult> EditScheduleAsync(TutoringSession Sess, int id)
		{
			var SessID = id;
			if (Sess == null || Sess.SessionId is null)
			{
				Sess = _dContext.Database.SqlQueryRaw<TutoringSession>($"usp_SessionInfo @SessionID",
				new SqlParameter("@SessionID", SessID)).AsEnumerable().First();
			}
			if (ModelState.IsValid)
			{
				try
				{
					var uinfo = User.Claims.ToArray();
					var em = await _dContext.Database.SqlQueryRaw<EmpIds>
							($"usp_SaveSession @StudentID,@GradeID,@PurchasedHrs,@TotalAmt,@StartTime,@EndTime,@Subjects,@TutorID,@SessionId,@UID,@Recurrence",
						new SqlParameter("@StudentID", Sess.StudentID),
							new SqlParameter("@GradeID", Sess.GradeID),
		new SqlParameter("@PurchasedHrs", (object)Sess.PurchasedHrs ?? DBNull.Value),
		new SqlParameter("@TotalAmt", (object)Sess.TotalAmt ?? DBNull.Value),
		new SqlParameter("@Recurrence", (object)Sess.Recurrence ?? DBNull.Value),
		new SqlParameter("@StartTime", (object)Sess.StartTime ?? DBNull.Value),
		new SqlParameter("@EndTime", (object)Sess.EndTime ?? DBNull.Value),
		new SqlParameter("@Subjects", string.Join(", ", Sess.Subjects)),
		new SqlParameter("@TutorID", Sess.TutorID),
		new SqlParameter("@SessionId", (object)Sess.SessionId ?? 0),
		new SqlParameter("@UID", uinfo[2].Value)
		).ToListAsync();
					SessID = em[0].EmpID;
					Sess.SessionId = SessID;
				}
				catch (Exception ex)
				{
					TempData["msg"] = "There was an error. " + Environment.NewLine + ex.Message;
				}
			}
			var _student = _dContext.Database.SqlQuery<Student>($"usp_StudentRecord").AsEnumerable().Select(p => new SelectListItem
			{
				Value = p.StudentID.ToString(),
				Text = p.StudentName.ToString(),
				Selected = p.StudentID.ToString() == Sess.StudentID.ToString() ? true : false
			}).ToList();
			var _emps = _dContext.Database.SqlQuery<StaffData>($"usp_StaffRecord").AsEnumerable().Where(p => p.empstatus == 1).Select(p => new SelectListItem
			{
				Value = p.empid.ToString(),
				Text = p.empname
			}).ToList();
			var _subjects = _dContext.Database.SqlQuery<Subjects>($"SELECT SubjectID, Subject FROM tblSubjects").AsEnumerable().Select(p => new SelectListItem
			{
				Value = p.SubjectID.ToString(),
				Text = p.Subject
			}).ToList();
			var grade = _dContext.Database.SqlQuery<Grade>($"SELECT GradeID, GradeLevel FROM tblGradeLevels").ToList();
			var SchLines = _dContext.Database.SqlQuery<SchedulesLines>($"usp_Schedules {SessID}").ToList();
			EmpLibrary empLibrary = new EmpLibrary();
			empLibrary.Sess = Sess;
			empLibrary.grade = grade;
			empLibrary.SchLines = SchLines;
			empLibrary.EmpList = _emps;
			empLibrary.SbjcList = _subjects;
			empLibrary.stuList = _student;
			return View(empLibrary);
		}
		[HttpGet]
		public async Task<ActionResult> Profile(int stuid)
		{
			IList<StudentProfile> lst = null;
			if (ModelState.IsValid)
			{
				var uinfo = User.Claims.ToArray();
				lst = await _dContext.Database.SqlQueryRaw<StudentProfile>
					($"usp_StudentProfile {stuid}").ToListAsync();
			}
			return Json(lst);
		}
		[HttpGet]
		public async Task<ActionResult> SchLines(int stuid)
		{
			IList<SchedulesLines> lst = null;
			if (ModelState.IsValid)
			{
				var uinfo = User.Claims.ToArray();
				lst = await _dContext.Database.SqlQueryRaw<SchedulesLines>
					($"usp_SchedulesByStudent {stuid}").ToListAsync();
			}
			return Json(lst);
		}
		[HttpGet]
		public async Task<ActionResult> PayLines(int stuid)
		{
			IList<PayLines> lst = null;
			if (ModelState.IsValid)
			{
				var uinfo = User.Claims.ToArray();
				lst = await _dContext.Database.SqlQueryRaw<PayLines>
					($"usp_PaymentDetails {stuid}").ToListAsync();
			}
			return Json(lst);
		}
		[HttpPost]
		public IActionResult Lists([FromBody] Dictionary<string, object> jsonData)
		{
			string s = jsonData["s"].ToString();
			string js = "{\"[Sub-Department]\":" + gM.Data2Json("SELECT Depart val,Depart txt FROM tblDepartments WHERE (ParentId IS NOT NULL)") + ",\"Designation\":" + gM.Data2Json("SELECT Designation val,Designation txt FROM tblDesignations") + ",\"EmpStatus\":[{\"val\":1,\"txt\":\"Active\"},{\"val\":0,\"txt\":\"Inactive\"}] }";
			return Ok(js);
		}
		[HttpGet]
		public ActionResult RemoveSch(string id)
		{
			gM.FillDSet($"DELETE FROM tblSessionSchedule WHERE  (Id = {id}) AND Stat='Scheduled'");
			return Ok("");
		}
		[HttpGet]
		public ActionResult SetPurchasedHrs(string PurchasedHrs, string AMT, string id)
		{
			gM.FillDSet($"UPDATE tblSessions SET PurchasedHrs = {PurchasedHrs}, TotalAmt = {AMT} WHERE  (SessionID = {id})");
			return Ok("");
		}
		[HttpGet]
		public ActionResult RemovePay(string id)
		{
			gM.FillDSet($"DELETE FROM tblStudentPayments WHERE  (Id = {id})");
			return Ok("");
		}
		public async Task<ActionResult> Expenses()
		{
			var expenses = await _dContext.Database.SqlQueryRaw<Expense>
				  ($"select * from tblExpenses").ToListAsync();
			return View(expenses);
		}
		public ActionResult ExpenseCreate()
		{
			var model = new Expense { Date = DateTime.Today };
			return PartialView("_ExpenseForm", model);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult ExpenseCreate(Expense expense)
		{
			if (ModelState.IsValid)
			{
				gM.FillDSet($"INSERT INTO tblExpenses (Date, Category, Amount, PaymentMethod, Notes, ReceiptNo) VALUES ('{expense.Date}','{expense.Category}',{expense.Amount},'{expense.PaymentMethod}','{expense.Notes}','{expense.ReceiptNo}')");
				return RedirectToAction("Expenses");
			}
			return PartialView("_ExpenseForm", expense);
		}
		public async Task<ActionResult> ExpenseEdit(int? id)
		{
			if (id == null) return NotFound();
			var expense = await _dContext.Database
	   .SqlQueryRaw<Expense>("SELECT * FROM tblExpenses WHERE Id = {0}", id)
	   .FirstOrDefaultAsync();
			if (expense == null) return NotFound();
			return PartialView("_ExpenseForm", expense);
		}
		public async Task<ActionResult> ExpenseDel(int? id)
		{
			gM.FillDSet($"delete tblExpenses WHERE Id = {id}");
			return RedirectToAction("Expenses");
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult ExpenseEdit(Expense expense)
		{
			if (ModelState.IsValid)
			{
				gM.FillDSet($"UPDATE tblExpenses SET Date ='{expense.Date}', Category = N'{expense.Category}', Amount = {expense.Amount}, PaymentMethod = N'{expense.PaymentMethod}', Notes = N'{expense.Notes}', ReceiptNo = N'{expense.ReceiptNo}' WHERE  (Id = {expense.Id})");
				return RedirectToAction("Expenses");
			}
			return PartialView("_ExpenseForm", expense);
		}
		public ActionResult ProfitReport()
		{ return View(); }
		public ActionResult GetIncomeVsExp(string month)
		{
			var json = gM.Data2Json($"EXEC usp_IncomeVsExp @rMonth = '{month}'");
			var list = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(json);
			return Json(list);
		}
		public ActionResult StudentPoints()
		{ return View(); }
		public ActionResult GetStudentPoints(string month)
		{
			var json = gM.Data2Json($"EXEC usp_StudentPoints @rMonth = '{month}'");
			var list = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(json);
			return Json(list);
		}
		public ActionResult WorkHoursCalculator()
		{
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
		public ActionResult GetWorkHoursCalculator(string month, string emp)
		{
			var json = gM.Data2Json($"EXEC usp_WorkHoursCalculator @mnth = '{month}',@emp={emp}");
			var list = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(json);
			return Json(list);
		}
	}
}
