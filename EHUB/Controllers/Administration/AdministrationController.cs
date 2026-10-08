using EHUB.Models.Administration;
using EHUB.Models.Attendance;
using EHUB.Models.Home;
using EHUB.Models.HRManagement;
using EHUB.Models.Leaves;
using EHUB.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Data;
namespace EHUB.Controllers.Administration
{
	[AuthUser]
	public class AdministrationController : BaseController
	{
		private readonly DataContext _dContext;
		private readonly IWebHostEnvironment _wHostEnv;
		private readonly GM _gm;
		GM gM = new GM();
		public AdministrationController(DataContext dContext, IWebHostEnvironment wHostEnv, GM gm)
			: base(dContext)
		{
			_dContext = dContext;
			_wHostEnv = wHostEnv;
			_gm = gm;
		}
		[AuthWrite]
		public ActionResult UserGroups()
		{
			var _UserGroups = new List<UserGroups>();
			if (ModelState.IsValid)
			{
				_UserGroups = _dContext.Database.SqlQuery<UserGroups>($"usp_UserGroups").ToList();
			}
			return View(_UserGroups);
		}
		[HttpGet]
		public async Task<ActionResult> ModuleListAsync(int grp)
		{
			IList<PageList> _ModuleList = null;
			if (ModelState.IsValid)
			{
				var uinfo = User.Claims.ToArray();
				_ModuleList = await _dContext.Database.SqlQueryRaw<PageList>
					($"usp_ModuleList @GroupID",
				new SqlParameter("@GroupID", grp)).ToListAsync();
			}
			return Json(_ModuleList);
		}
		[HttpPost]
		public ActionResult SaveGroupAsync(UserGroups grp)
		{
			//if (ModelState.IsValid)
			//{
			var uinfo = User.Claims.ToArray();
			var _UserGroups = new List<UserGroups>();
			_UserGroups = _dContext.Database.SqlQueryRaw<UserGroups>($"usp_SaveGroup @GroupName,@GroupID, @UID", new SqlParameter("@GroupName", grp.GroupName)
				, new SqlParameter("@GroupID", grp.GroupID)
				, new SqlParameter("@UID", uinfo[2].Value)).ToList();
			//}
			return RedirectToAction("UserGroups", "Administration");
		}
		[HttpGet]
		public async Task<ActionResult> SetRoleAsync(int grp, int pgid, int tblid, int role)
		{
			if (ModelState.IsValid)
			{
				var _UserGroups = new List<UserGroups>();
				if (ModelState.IsValid)
				{
					_UserGroups = _dContext.Database.SqlQueryRaw<UserGroups>($"usp_SetRole @grp,@pgid, @tblid,@role", new SqlParameter("@grp", grp)
						, new SqlParameter("@pgid", pgid)
						, new SqlParameter("@tblid", tblid)
						, new SqlParameter("@role", role)).ToList();
				}
			}
			return RedirectToAction("UserGroups", "Administration");
		}
		[AuthWrite]
		public ActionResult Departments()
		{
			var _staff = _dContext.Database.SqlQuery<StaffData>($"usp_StaffRecord").ToList();
			var _staffhead = gM.FillDSet($"SELECT NULL DepartID, '- Nothing -' Depart union SELECT DepartID, Depart FROM tblDepartments WHERE (ParentId IS NULL)").Tables[0].Rows;
			var _regDepats = gM.FillDSet($"SELECT tblDepartments.DepartID, tblDepartments.Depart, tblDepartments.ParentId, tblDepartments.HeadEmp, pDepat.Depart AS dHead, tblEmployees.FName, tblEmployees.LName, tblEmployees.EmpCode\r\nFROM     tblDepartments LEFT OUTER JOIN\r\n                  tblEmployees ON tblDepartments.HeadEmp = tblEmployees.EmpID LEFT OUTER JOIN\r\n                  tblDepartments AS pDepat ON tblDepartments.ParentId = pDepat.DepartID WHERE  (tblDepartments.ParentId IS NOT NULL)ORDER BY tblDepartments.ParentId DESC").Tables[0].Rows;
			var _hDepats = gM.FillDSet($"SELECT tblDepartments.DepartID, tblDepartments.Depart, tblDepartments.ParentId, tblDepartments.HeadEmp, pDepat.Depart AS dHead, tblEmployees.FName, tblEmployees.LName, tblEmployees.EmpCode\r\nFROM     tblDepartments LEFT OUTER JOIN\r\n                  tblEmployees ON tblDepartments.HeadEmp = tblEmployees.EmpID LEFT OUTER JOIN\r\n                  tblDepartments AS pDepat ON tblDepartments.ParentId = pDepat.DepartID WHERE  (tblDepartments.ParentId IS NULL) ORDER BY tblDepartments.ParentId DESC").Tables[0].Rows;
			var data = new List<object>();
			data.Add(_staff);
			data.Add(_staffhead);
			data.Add(_regDepats);
			data.Add(_hDepats);
			return View(data);
		}
		[HttpPost]
		public ActionResult DepartmentsAsync(Departments deprt)
		{
			//if (ModelState.IsValid)
			//{
			var uinfo = User.Claims.ToArray();
			var empid = _dContext.Database.SqlQueryRaw<EmpIds>($"usp_SaveDepart @Depart,@ParentId,@HeadEmp, @UID,@DepartID", new SqlParameter("@Depart", deprt.Depart)
				, new SqlParameter("@ParentId", (object)deprt.ParentId ?? DBNull.Value)
					, new SqlParameter("@HeadEmp", (object)deprt.HeadEmp ?? DBNull.Value)
				, new SqlParameter("@UID", uinfo[2].Value),
				new SqlParameter("@DepartID", deprt.DepartID)).ToList();
			//}
			return RedirectToAction("Departments", "Administration");
		}
		[AuthWrite]
		public ActionResult Locations()
		{
			var _prov = gM.FillDSet($" SELECT 0 LocId, '- Nothing -' Loc union SELECT LocId, Loc FROM tblLocations WHERE  (LocType = 1)").Tables[0].Rows;
			var _loc = gM.FillDSet($"SELECT tblLocations.LocId, tblLocations.Loc, tblLocations.pLocId,  prov.Loc AS prov FROM tblLocations LEFT OUTER JOIN tblLocations AS prov ON tblLocations.pLocId = prov.LocId where tblLocations.pLocId <>0").Tables[0].Rows;
			var data = new List<object>();
			data.Add(_prov);
			data.Add(_loc);
			return View(data);
		}
		[HttpPost]
		public ActionResult Locations(Locations loc)
		{
			//if (ModelState.IsValid)
			//{
			var uinfo = User.Claims.ToArray();
			var empid = _dContext.Database.SqlQueryRaw<EmpIds>($"usp_SaveLoc @Loc,@pLocId, @UID,@LocID", new SqlParameter("@Loc", loc.Loc)
				, new SqlParameter("@pLocId", (object)loc.pLocId ?? DBNull.Value)
				, new SqlParameter("@UID", uinfo[2].Value),
				new SqlParameter("@LocID", loc.LocId)).ToList();
			//}
			return RedirectToAction("Locations", "Administration");
		}
		public ActionResult SystemLibrary()
		{
			return View();
		}
		[AuthWrite]
		public ActionResult WorkDays()
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
			pgRoster _pg = new pgRoster();
			_pg.departs = dd;
			IList<StaffData> _staff = null;
			_staff = _dContext.Database.SqlQuery<StaffData>($"usp_StaffRecord").ToList();
			_pg.staff = _staff.Where(x => x.empstatus == 1).ToList();
			return View(_pg);
		}
		[HttpPost]
		public async Task<ActionResult> WorkDays(WorkDay workday)
		{
			var uinfo = User.Claims.ToArray();
			if (ModelState.IsValid)
			{
				try
				{
					string Ids;
					if (workday.mode == 1)
					{
						Ids = string.Join(",", workday.DepartIds);
					}
					else { Ids = string.Join(",", workday.EmpIds); }
					var lv = await _dContext.Database.SqlQueryRaw<LvIds>
							($"usp_SaveWorkDay @Mode,@Ids,@fdate,@tdate,@state,@descs,@UserID,@Recurring,@option",
						new SqlParameter("@Mode", workday.mode),
							new SqlParameter("@Ids", Ids),
							new SqlParameter("@fdate", workday.fdate),
							new SqlParameter("@tdate", workday.tdate),
							new SqlParameter("@state", workday.state),
							new SqlParameter("@descs", (string)workday.descs ?? ""),
						new SqlParameter("@UserID", uinfo[2].Value),
						new SqlParameter("@Recurring", workday.Recurring),
new SqlParameter("@option", workday.option)
							).ToListAsync();
					var LvId = lv[0].LvId;
				}
				catch (Exception ex)
				{
					TempData["msg"] = "There was an error. " + Environment.NewLine + ex.Message;
				}
			}
			return RedirectToAction("WorkDays", "Administration");
		}
		[HttpGet]
		public async Task<ActionResult> GetWorkDays(int id)
		{
			IList<WDayInfo> edu = null;
			if (ModelState.IsValid)
			{
				var uinfo = User.Claims.ToArray();
				edu = await _dContext.Database.SqlQueryRaw<WDayInfo>
					($"usp_WkDayInfo @EmpId",
				new SqlParameter("@EmpId", id)).ToListAsync();
			}
			return Json(edu);
		}
		[HttpGet]
		public async Task<ActionResult> DelWorkDaysAsync(string id)
		{
			IList<WDayInfo> edu = null;
			if (ModelState.IsValid)
			{
				var uinfo = User.Claims.ToArray();
				edu = await _dContext.Database.SqlQueryRaw<WDayInfo>
					($"usp_WkDayDel @Id,@UserID",
				new SqlParameter("@UserID", uinfo[2].Value),
				new SqlParameter("@id", id)).ToListAsync();
			}
			return Json(edu);
		}
		[HttpGet]
		public async Task<ActionResult> DelHeadAsync(string id)
		{
			string ct = "0";
			if (ModelState.IsValid)
			{
				ct = gM.FillDSet("usp_DepartDel " + id).Tables[0].Rows[0][0].ToString();
			}
			return Json(ct);
		}
		[HttpGet]
		public async Task<ActionResult> DelGroupAsync(string id)
		{
			var uinfo = User.Claims.ToArray();
			if (ModelState.IsValid)
			{
				gM.FillDSet($"usp_UGroupDel {id},{uinfo[2].Value}");
			}
			return RedirectToAction("UserGroups", "Administration");
		}
		[HttpGet]
		public async Task<ActionResult> DelDesigAsync(string id)
		{
			var uinfo = User.Claims.ToArray();
			if (ModelState.IsValid)
			{
				gM.FillDSet($"DELETE FROM tblDesignations WHERE (Id = {id})");
			}
			return RedirectToAction("Designations", "Administration");
		}
		[AuthWrite]
		public ActionResult ExemptedDay()
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
			pgRoster _pg = new pgRoster();
			_pg.departs = dd;
			IList<StaffData> _staff = null;
			_staff = _dContext.Database.SqlQuery<StaffData>($"usp_StaffRecord").ToList();
			var exemptTypes = _dContext.Database.SqlQuery<ExemptTypes>($"SELECT ExemTypeId, ExemType FROM tblExemTypes").ToList();
			_pg.staff = _staff.Where(x => x.empstatus == 1).ToList();
			_pg.exptTypes = exemptTypes;
			return View(_pg);
		}
		[HttpPost]
		[AuthWrite]
		public async Task<ActionResult> ExemptedDay(ExemptedDay workday)
		{
			var uinfo = User.Claims.ToArray();
			if (ModelState.IsValid)
			{
				try
				{
					string Ids;
					if (workday.mode == 1)
					{
						Ids = string.Join(",", workday.DepartIds);
					}
					else { Ids = string.Join(",", workday.EmpIds); }
					var lv = await _dContext.Database.SqlQueryRaw<LvIds>
							($"usp_SaveExmptDay @Mode,@Ids,@dtExempt,@ExemTypeId,@ExptMin,@Remks,@UserID",
						new SqlParameter("@Mode", workday.mode),
							new SqlParameter("@Ids", Ids),
							new SqlParameter("@dtExempt", workday.dtExempt),
							new SqlParameter("@ExemTypeId", workday.ExemTypeId),
							new SqlParameter("@ExptMin", workday.ExptMin),
							new SqlParameter("@Remks", (string)workday.Remks ?? ""),
						new SqlParameter("@UserID", uinfo[2].Value)
							).ToListAsync();
					var LvId = lv[0].LvId;
				}
				catch (Exception ex)
				{
					TempData["msg"] = "There was an error. " + Environment.NewLine + ex.Message;
				}
			}
			return RedirectToAction("ExemptedDay", "Administration");
		}
		[HttpGet]
		public async Task<ActionResult> GetExemptedDay(int id)
		{
			IList<ExemptInfo> edu = null;
			if (ModelState.IsValid)
			{
				var uinfo = User.Claims.ToArray();
				edu = await _dContext.Database.SqlQueryRaw<ExemptInfo>
					($"usp_ExmptDayInfo @EmpId",
				new SqlParameter("@EmpId", id)).ToListAsync();
			}
			return Json(edu);
		}
		[HttpGet]
		public async Task<ActionResult> DelExemptedDayAsync(string id)
		{
			IList<WDayInfo> edu = null;
			if (ModelState.IsValid)
			{
				var uinfo = User.Claims.ToArray();
				edu = await _dContext.Database.SqlQueryRaw<WDayInfo>
					($"usp_RosterDel @Id,@UserID",
				new SqlParameter("@UserID", uinfo[2].Value),
				new SqlParameter("@id", id)).ToListAsync();
			}
			return Json(edu);
		}
		[AuthWrite]
		public ActionResult ShiftPlans()
		{
			var _shifts = gM.FillDSet($"SELECT shiftId, shiftName, InTime, OutTime, late, early, shortday, halfday,(SELECT shiftId, shiftName, InTime, OutTime, late, early, shortday, halfday FROM tblShifts where shiftId=sh.shiftId for json path)jdata FROM tblShifts sh WHERE (isDel = 0)").Tables[0].Rows;
			return View(_shifts);
		}
		[HttpPost]
		public ActionResult ShiftPlanAsync(ShiftPlan plan)
		{
			//if (ModelState.IsValid)
			//{
			var uinfo = User.Claims.ToArray();
			var empid = _dContext.Database.SqlQueryRaw<EmpIds>($"usp_SaveShiftPlan @shiftName, @InTime, @OutTime, @late, @early, @shortday, @halfday,@UID,@shiftId", new SqlParameter("@shiftName", plan.shiftName.TrimStart())
				, new SqlParameter("@InTime", plan.InTime)
					, new SqlParameter("@OutTime", plan.OutTime)
				, new SqlParameter("@late", (object)plan.late ?? 0)
				, new SqlParameter("@early", (object)plan.early ?? 0)
				, new SqlParameter("@shortday", (object)plan.shortday ?? 0)
				, new SqlParameter("@halfday", (object)plan.halfday ?? 0)
				, new SqlParameter("@UID", uinfo[2].Value),
				new SqlParameter("@shiftId", plan.shiftId)).ToList();
			//}
			return RedirectToAction("ShiftPlans", "Administration");
		}
		[AuthWrite]
		public ActionResult Designations()
		{
			var _Departments = _dContext.Database.SqlQuery<Departments>($"SELECT * FROM tblDepartments order by Depart").ToList();
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
			var _staff = _dContext.Database.SqlQuery<StaffData>($"usp_StaffRecord").ToList();
			var _des = gM.FillDSet($"SELECT Id, Designation FROM tblDesignations").Tables[0];
			var list = _des.Rows.Cast<DataRow>().ToList();
			pgRoster _pg = new pgRoster
			{
				staff = _staff,
				departs = dd,
				design = list
			};
			return View(_pg);
		}
		[HttpPost]
		public ActionResult Designations(Desigs des)
		{
			//if (ModelState.IsValid)
			//{
			var uinfo = User.Claims.ToArray();
			var empid = _dContext.Database.SqlQueryRaw<EmpIds>($"usp_SaveDesign @Designation, @UID,@Id", new SqlParameter("@Designation", des.Designation)
				, new SqlParameter("@Id", des.Id)
				, new SqlParameter("@UID", uinfo[2].Value)).ToList();
			//}
			return RedirectToAction("Designations", "Administration");
		}
		[HttpPost]
		public async Task<ActionResult> ManagerAssignment(MgrAssign workday)
		{
			var uinfo = User.Claims.ToArray();
			if (ModelState.IsValid)
			{
				try
				{
					string Ids;
					if (workday.mode == 1)
					{
						Ids = string.Join(",", workday.DepartIds);
					}
					else { Ids = string.Join(",", workday.EmpIds); }
					var lv = await _dContext.Database.SqlQueryRaw<LvIds>
							($"usp_AssignLineMgr @Mode,@Ids,@MgrId",
						new SqlParameter("@Mode", workday.mode),
							new SqlParameter("@Ids", Ids),
							new SqlParameter("@MgrId", workday.MgrId)
							).ToListAsync();
				}
				catch (Exception ex)
				{
					TempData["msg"] = "There was an error. " + Environment.NewLine + ex.Message;
				}
			}
			return RedirectToAction("Designations", "Administration");
		}
		[AuthWrite]
		public ActionResult ApproveLevels()
		{
			IList<StaffData> _staff = _dContext.Database.SqlQuery<StaffData>($"usp_StaffRecord").ToList();
			var staff = _staff.Where(x => x.empstatus == 1).ToList();
			var staticOptions = new List<SelectListItem>
			{
				new SelectListItem { Value = "0", Text = "Line Manager" },
				new SelectListItem { Value = "1", Text = "Department Manager" }
			};
			var dbOptions = staff
			 .Select(p => new SelectListItem
			 {
				 Value = p.empid.ToString(),
				 Text = p.empcode + " " + p.empname
			 })
			 .ToList();
			var combinedOptions = staticOptions.Concat(dbOptions).ToList();
			var lleave = gM.FillDSet($"SELECT approv_by, apriority FROM tblApprovePriority WHERE (approv_grp = 'leave') ORDER BY apriority").Tables[0].Rows;
			var data = new List<object>();
			data.Add(combinedOptions);
			data.Add(lleave);
			return View(data);
		}
		[HttpPost]
		[AuthWrite]
		public async Task<ActionResult> ApproveLevels(aprLevel alevel)
		{
			gM.FillDSet("DELETE FROM tblApprovePriority WHERE (approv_grp = 'leave')");
			for (int i = 0; i < alevel.item.Length; i++)
			{
				int item = alevel.item[i];
				gM.FillDSet($"INSERT INTO tblApprovePriority (approv_grp, approv_by, apriority) VALUES ('leave', {item}, {i})");
			}
			return RedirectToAction("ApproveLevels", "Administration");
		}
		public ActionResult GradeLevels()
		{
			var _GradeLevels = _dContext.Database.SqlQuery<GradeLevels>($"select * from tblGradeLevels").ToList();
			return View(_GradeLevels);
		}
		[HttpPost]
		public ActionResult GradeLevelsAsync(GradeLevels _GradeLevels)
		{
			//if (ModelState.IsValid)
			//{
			var uinfo = User.Claims.ToArray();
			if (_GradeLevels.GradeID == 0)
			{
				gM.FillDSet($"INSERT INTO tblGradeLevels (GradeLevel) VALUES ('{_GradeLevels.GradeLevel}')");
			}
			else
			{
				gM.FillDSet($"UPDATE tblGradeLevels SET GradeLevel = '{_GradeLevels.GradeLevel}' WHERE (GradeID = {_GradeLevels.GradeID})");
			}
			//}
			return RedirectToAction("GradeLevels", "Administration");
		}
		public ActionResult Subjects()
		{
			var _sub = _dContext.Database.SqlQuery<SubjectList>($"select * from tblSubjects").ToList();
			return View(_sub);
		}
		[HttpPost]
		public ActionResult SubjectsAsync(SubjectList _subj)
		{
			//if (ModelState.IsValid)
			//{
			var uinfo = User.Claims.ToArray();
			if (_subj.SubjectID == 0)
			{
				gM.FillDSet($"INSERT INTO tblSubjects (Subject) VALUES ('{_subj.Subject}')");
			}
			else
			{
				gM.FillDSet($"UPDATE tblSubjects SET Subject = '{_subj.Subject}' WHERE (SubjectID = {_subj.SubjectID})");
			}
			//}
			return RedirectToAction("Subjects", "Administration");
		}
		public ActionResult BulkEmail()
		{
			TempData["msg"] = "";
			string Audience = gM.Data2Json("usp_AudienceEmail");
			return View("BulkEmail", Audience);
		}
		[HttpPost]
		public ActionResult BulkEmailAsync(EmailMessage _msg)
		{
			if (ModelState.IsValid)
			{
				try
				{
					var uinfo = User.Claims.ToArray();
					foreach (var mail in _msg.Recipients)
					{
						gM.SendEmail(mail, _msg.Subject, _msg.MailBody, _msg.Attachment);
					}
					TempData["msg"] = _msg.Recipients.Count.ToString() + " emails have been sent successfully.";
				}
				catch (Exception ex)
				{
					TempData["msg"] = "There was an error. " + Environment.NewLine + ex.Message;
				}
			}
			return RedirectToAction("BulkEmail", "Administration");
		}
	}
}
