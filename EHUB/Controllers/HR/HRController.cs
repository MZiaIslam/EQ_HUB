using EHUB.Models.Administration;
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
	public class HRController : Controller
	{
		private readonly DataContext _dContext;
		private readonly IWebHostEnvironment _wHostEnv;
		private readonly GM _gm;
		private readonly LocationScope _locScope;
		GM gM = new GM();
		public HRController(DataContext dContext, IWebHostEnvironment wHostEnv, GM gm, LocationScope locScope)
		{
			_locScope = locScope;
			_dContext = dContext;
			_wHostEnv = wHostEnv;
			_gm = gm;
		}
		/// <summary>
		/// Replaces the staff member's location assignments within the set the signed-in user manages
		/// (all active locations for administrators, otherwise their own locations).
		/// </summary>
		private void SaveEmpLocations(int empId, int[]? selected)
		{
			if (empId <= 0 || Request.Method != "POST") return;
			var managed = _locScope.Allowed.Select(l => l.LocationId).ToList();
			if (managed.Count == 0) return;
			var keep = (selected ?? Array.Empty<int>()).Where(managed.Contains).Distinct().ToList();
			var uid = User.Claims.ToArray()[2].Value;
			var managedCsv = string.Join(",", managed);
			_dContext.Database.ExecuteSqlRaw(
				"DELETE FROM tblEmpWorkLocations WHERE EmpID = @e AND LocationId IN (" + managedCsv + ")" +
				(keep.Count > 0 ? " AND LocationId NOT IN (" + string.Join(",", keep) + ")" : ""),
				new SqlParameter("@e", empId));
			foreach (var locId in keep)
			{
				_dContext.Database.ExecuteSqlRaw(
					"IF NOT EXISTS (SELECT 1 FROM tblEmpWorkLocations WHERE EmpID = @e AND LocationId = @l) INSERT INTO tblEmpWorkLocations (EmpID, LocationId, CreatedBy) VALUES (@e, @l, @u)",
					new SqlParameter("@e", empId), new SqlParameter("@l", locId), new SqlParameter("@u", uid));
			}
		}
		private void FillLocationLibrary(EmpLibrary lib, int empId)
		{
			lib.workLocations = _locScope.Allowed;
			try
			{
				lib.empLocationIds = _dContext.Database.SqlQueryRaw<int>(
					"SELECT LocationId AS [Value] FROM tblEmpWorkLocations WHERE EmpID = @e", new SqlParameter("@e", empId)).ToList();
			}
			catch
			{
				lib.empLocationIds = new List<int>();
			}
		}
		[AuthWrite]
		public ActionResult StaffDataReport()
		{
			return View();
		}
		[HttpPost]
		public IActionResult StaffRecord(Fillterdata fillterdata)
		{
			var _emps = _locScope.FilterStaff(_dContext.Database.SqlQuery<StaffData>($"usp_StaffRecord").AsEnumerable()).Select(p => new SelectListItem
			{
				Value = p.empid.ToString(),
				Text = p.empcode.ToString() + " " + p.empname,
				Selected = p.empid.ToString() == fillterdata.emp ? true : false
			}).ToList();
			var _staffdata = new List<StaffData>();
			if (ModelState.IsValid)
			{
				_staffdata = _locScope.FilterStaff(_dContext.Database.SqlQuery<StaffData>($"usp_StaffRecord").ToList());
			}
			_staffdata = _staffdata.Where(x => x.empstatus.ToString() == fillterdata.empstat).ToList();
			if (fillterdata.emp != "0")
			{
				_staffdata = _staffdata.Where(x => x.empid.ToString() == fillterdata.emp).ToList();
			}
			if (fillterdata.chkdt == "on")
			{
				_staffdata = _staffdata.Where(x => x.joiningdate >= fillterdata.fromdate && x.joiningdate <= fillterdata.todate).ToList();
			}
			var fillters = new fillters();
			fillters.emps = _emps;
			fillters.linemgr = _staffdata;
			fillters.fills = fillterdata;
			return View(fillters);
		}
		[AuthWrite]
		public IActionResult StaffRecord()
		{
			var _emps = _locScope.FilterStaff(_dContext.Database.SqlQuery<StaffData>($"usp_StaffRecord").AsEnumerable()).Select(p => new SelectListItem
			{
				Value = p.empid.ToString(),
				Text = p.empcode.ToString() + " " + p.empname
			}).ToList();
			var _staffdata = new List<StaffData>();
			var fillters = new fillters();
			fillters.emps = _emps;
			fillters.linemgr = _staffdata;
			return View(fillters);
		}
		[HttpPost]
		[HttpGet]
		[AuthWrite]
		public async Task<ActionResult> AddNewStaffAsync(Empolyee emp)
		{
			var EmpID = emp.EmpID;
			if (emp.EmpCode == 0)
			{
				emp.EmpCode = _dContext.Database.SqlQueryRaw<int>($"SELECT MAX(EmpCode)+1 FROM tblEmployees").ToList()[0];
			}
			if (ModelState.IsValid)
			{
				try
				{
					var uinfo = User.Claims.ToArray();
					var em = await _dContext.Database.SqlQueryRaw<EmpIds>
							($"usp_SaveEmp @EmpID,@EmpCode,@FName,@LName,@FatherName,@MobileNo,@Email,@Marital,@Gender,@DateOfBirth,@Address,@ContactPerson,@Relationship,@ContactNo,@GroupID,@Designation,@UserID,@JoiningDate,@permanentaddress,@remarks",
						new SqlParameter("@EmpID", emp.EmpID),
							new SqlParameter("@EmpCode", emp.EmpCode),
		new SqlParameter("@FName", emp.FName),
		new SqlParameter("@LName", (object)emp.LName ?? DBNull.Value),
		new SqlParameter("@FatherName", (object)emp.FatherName ?? DBNull.Value),
		new SqlParameter("@MobileNo", (object)emp.MobileNo ?? DBNull.Value),
		new SqlParameter("@Email", (object)emp.Email ?? DBNull.Value),
		new SqlParameter("@Marital", emp.Marital),
		new SqlParameter("@Gender", emp.Gender),
		new SqlParameter("@DateOfBirth", (object)emp.DateOfBirth ?? DBNull.Value),
		new SqlParameter("@Address", (object)emp.Address ?? DBNull.Value),
		new SqlParameter("@ContactPerson", (object)emp.ContactPerson ?? DBNull.Value),
		new SqlParameter("@Relationship", (object)emp.Relationship ?? DBNull.Value),
		new SqlParameter("@ContactNo", (object)emp.ContactNo ?? DBNull.Value),
		new SqlParameter("@GroupID", (object)emp.GroupID ?? DBNull.Value),
		new SqlParameter("@Designation", (object)emp.Designation ?? DBNull.Value),
		new SqlParameter("@UserID", uinfo[2].Value),
		new SqlParameter("@JoiningDate", (object)emp.JoiningDate ?? DBNull.Value),
		new SqlParameter("@permanentaddress", (object)emp.permanentaddress ?? DBNull.Value),
		new SqlParameter("@remarks", (object)emp.remarks ?? DBNull.Value)
		).ToListAsync();
					EmpID = em[0].EmpID;
					emp.EmpID = EmpID;
					SaveEmpLocations(Convert.ToInt32(EmpID), emp.LocationIds);
					if (emp.photo != null)
					{
						var path = Path.Combine(_wHostEnv.WebRootPath + "/dist/img/StaffData/pics/" + EmpID + ".png");
						emp.photo.CopyTo(new FileStream(path, FileMode.Create));
					}
				}
				catch (Exception ex)
				{
					TempData["msg"] = "There was an error. " + Environment.NewLine + ex.Message;
				}
			}
			IList<StaffData> _LineMgr = null;
			IList<Departments> _Departments = null;
			var _StaffEdu = new List<StaffEdu>();
			var _staffdocs = new List<StaffDocs>();
			var _StaffWork = new List<StaffWorkHis>();
			var _Stations = new List<Stations>();
			var _Designations = new List<Designations>();
			var _JobStatus = new List<JobStatus>();
			var _UserGroups = new List<UserGroups>();
			var _Locations = new List<Locations>();
			//var EmpLibrary = new List<EmpLibrary>();
			EmpLibrary empLibrary = new EmpLibrary();
			empLibrary.emp = emp;
			_LineMgr = _dContext.Database.SqlQuery<StaffData>($"usp_StaffRecord").ToList();
			empLibrary.linemgr = _LineMgr.Where(x => x.empstatus == 1).ToList();
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
			empLibrary.departs = dd;
			_Stations = _dContext.Database.SqlQuery<Stations>($"SELECT * FROM tblStations").ToList();
			empLibrary.stations = _Stations.Where(x => x.Stat == 1).ToList();
			_Designations = _dContext.Database.SqlQuery<Designations>($"SELECT * FROM [dbo].[tblDesignations] order by Designation").ToList();
			empLibrary.desig = _Designations;
			_JobStatus = _dContext.Database.SqlQuery<JobStatus>($"SELECT  * FROM tblEmployeeStatus").ToList();
			empLibrary.jobstatus = _JobStatus;
			_UserGroups = _dContext.Database.SqlQuery<UserGroups>($"SELECT * FROM tblUserGroups where isdel=0 order by GroupName").ToList();
			empLibrary.usergroups = _UserGroups;
			_Locations = _dContext.Database.SqlQuery<Locations>($"SELECT * FROM tblLocations order by Loc").ToList();
			empLibrary.loc = _Locations.Where(x => x.LocType == 2).ToList();
			FillLocationLibrary(empLibrary, Convert.ToInt32(EmpID));
			_StaffEdu = _dContext.Database.SqlQueryRaw<StaffEdu>($"SELECT Id,Qualification, Institute, Description,EmpID FROM tblAcademicInfo where EmpID=@EmpID AND isDel=0",
				new SqlParameter("@EmpID", EmpID)).ToList();
			empLibrary.staffedus = _StaffEdu.ToList();
			_StaffWork = _dContext.Database.SqlQueryRaw<StaffWorkHis>($"usp_getStaffWorkHis @EmpID",
				new SqlParameter("@EmpID", EmpID)).ToList();
			empLibrary.workhist = _StaffWork.ToList();
			_staffdocs = _dContext.Database.SqlQueryRaw<StaffDocs>($"SELECT EmpID, Title, Type, DocName, Id FROM tblEmpDocuments WHERE (EmpID = @EmpID) AND isDel=0",
				new SqlParameter("@EmpID", EmpID)).ToList();
			empLibrary.staffdocs = _staffdocs.ToList();
			return View(empLibrary);
		}
		[HttpPost]
		public async Task<ActionResult> SaveEduAsync(StaffEdu ed)
		{
			IList<StaffEdu> edu = null;
			if (ModelState.IsValid)
			{
				var uinfo = User.Claims.ToArray();
				edu = await _dContext.Database.SqlQueryRaw<StaffEdu>
					($"usp_StaffEdu @EmpID,@UserID,@Qualification,@Institute,@Description,@Id",
				new SqlParameter("@EmpID", ed.empID),
				new SqlParameter("@UserID", uinfo[2].Value),
				new SqlParameter("@Qualification", ed.qualification),
				new SqlParameter("@Institute", ed.institute),
				new SqlParameter("@Description", (object)ed.description ?? ""),
				new SqlParameter("@Id", (object)ed.id ?? DBNull.Value)).ToListAsync();
			}
			return Json(edu);
		}
		[HttpPost]
		public async Task<ActionResult> SaveDocsAsync(StaffDocs dc)
		{
			IList<StaffDocs> _docs = null;
			string dir = dc.empID + "" + DateTime.Now.ToString("yyyyMMddHHmmss");
			//string file = DateTime.Now.ToString("yyyyMMddHHmmss")+"/"+dc.doc.FileName;
			if (ModelState.IsValid)
			{
				var uinfo = User.Claims.ToArray();
				_docs = await _dContext.Database.SqlQueryRaw<StaffDocs>
					($"usp_StaffDocs @EmpID,@UserID,@Title,@Type,@DocName,@Id",
				new SqlParameter("@EmpID", dc.empID),
				new SqlParameter("@UserID", uinfo[2].Value),
				new SqlParameter("@Title", dc.title),
				new SqlParameter("@Type", dc.type),
				new SqlParameter("@DocName", (dir + "/" + dc.doc.FileName)),
				new SqlParameter("@Id", (object)dc.id ?? DBNull.Value)).ToListAsync();
				if (dc.doc != null)
				{
					bool exists = System.IO.Directory.Exists(_wHostEnv.WebRootPath + "/dist/img/StaffData/docs/" + dir);
					if (!exists)
						System.IO.Directory.CreateDirectory(_wHostEnv.WebRootPath + "/dist/img/StaffData/docs/" + dir);
					var path = Path.Combine(_wHostEnv.WebRootPath + "/dist/img/StaffData/docs/" + dir + "/" + dc.doc.FileName);
					dc.doc.CopyTo(new FileStream(path, FileMode.Create));
				}
			}
			//await EditStaffAsync(null, dc.empID);
			return RedirectToAction("EditStaff", "HR", new { id = dc.empID });
			//return Json(_docs);
		}
		[HttpPost]
		public async Task<ActionResult> DeactivateAsync(DeactivateInfo dInfo)
		{
			if (ModelState.IsValid)
			{
				var uinfo = User.Claims.ToArray();
				var _docs = await _dContext.Database.SqlQueryRaw<DeactivateInfo>
					($"usp_DeactivateEmp @EmpID,@UserID,@Stat,@tDate,@rDate,@Descs",
				new SqlParameter("@EmpID", dInfo.empID),
				new SqlParameter("@UserID", uinfo[2].Value),
				new SqlParameter("@Stat", dInfo.stat),
				new SqlParameter("@tDate", dInfo.tdate),
				new SqlParameter("@rDate", dInfo.rdate),
				new SqlParameter("@Descs", (string)dInfo.descs ?? "")).ToListAsync();
			}
			return RedirectToAction("EditStaff", "HR", new { id = dInfo.empID });
		}
		[HttpGet]
		public async Task<ActionResult> DelEduAsync(string id, string empid)
		{
			IList<StaffEdu> edu = null;
			if (ModelState.IsValid)
			{
				var uinfo = User.Claims.ToArray();
				edu = await _dContext.Database.SqlQueryRaw<StaffEdu>
					($"usp_StaffEduDel @EmpID,@Id,@UserID",
				new SqlParameter("@EmpID", empid),
				new SqlParameter("@UserID", uinfo[2].Value),
				new SqlParameter("@id", id)).ToListAsync();
			}
			return Json(edu);
		}
		[HttpGet]
		public async Task<ActionResult> DelWorkAsync(string id, string empid)
		{
			IList<StaffWorkHis> edu = null;
			if (ModelState.IsValid)
			{
				var uinfo = User.Claims.ToArray();
				edu = await _dContext.Database.SqlQueryRaw<StaffWorkHis>
					($"usp_WokHisDel @EmpID,@Id,@UserID",
				new SqlParameter("@EmpID", empid),
				new SqlParameter("@UserID", uinfo[2].Value),
				new SqlParameter("@id", id)).ToListAsync();
			}
			return Json(edu);
		}
		[HttpGet]
		public async Task<ActionResult> DelDocAsync(string id, string empid)
		{
			IList<StaffDocs> docs = null;
			if (ModelState.IsValid)
			{
				var uinfo = User.Claims.ToArray();
				docs = await _dContext.Database.SqlQueryRaw<StaffDocs>
					($"usp_StaffDocDel @EmpID,@Id,@UserID",
				new SqlParameter("@EmpID", empid),
				new SqlParameter("@UserID", uinfo[2].Value),
				new SqlParameter("@id", id)).ToListAsync();
			}
			return Json(docs);
		}
		[HttpPost]
		[HttpGet]
		[AuthWrite]
		public async Task<ActionResult> EditStaffAsync(Empolyee emp, int id)
		{
			var EmpID = id;
			if (emp == null || emp.EmpID == 0)
			{
				emp = _dContext.Database.SqlQueryRaw<Empolyee>($"select EmpID,EmpCode, FName, LName, FatherName, MobileNo, Email, Marital, Gender, DateOfBirth, Address, ContactPerson,Relationship, ContactNo, GroupID, Designation, CreatedBy, JoiningDate, EmpStatus,permanentaddress, remarks from tblEmployees where EmpID=@EmpID",
				new SqlParameter("@EmpID", EmpID)).First();
			}
			if (ModelState.IsValid)
			{
				try
				{
					var uinfo = User.Claims.ToArray();
					var em = await _dContext.Database.SqlQueryRaw<EmpIds>
							($"usp_SaveEmp @EmpID,@EmpCode,@FName,@LName,@FatherName,@MobileNo,@Email,@Marital,@Gender,@DateOfBirth,@Address,@ContactPerson,@Relationship,@ContactNo,@GroupID,@Designation,@UserID,@JoiningDate,@permanentaddress,@remarks",
		new SqlParameter("@EmpID", emp.EmpID),
		new SqlParameter("@EmpCode", emp.EmpCode),
		new SqlParameter("@FName", emp.FName),
		new SqlParameter("@LName", (object)emp.LName ?? DBNull.Value),
		new SqlParameter("@FatherName", (object)emp.FatherName ?? DBNull.Value),
		new SqlParameter("@MobileNo", (object)emp.MobileNo ?? DBNull.Value),
		new SqlParameter("@Email", (object)emp.Email ?? DBNull.Value),
		new SqlParameter("@Marital", emp.Marital),
		new SqlParameter("@Gender", emp.Gender),
		new SqlParameter("@DateOfBirth", (object)emp.DateOfBirth ?? DBNull.Value),
		new SqlParameter("@Address", (object)emp.Address ?? DBNull.Value),
		new SqlParameter("@ContactPerson", (object)emp.ContactPerson ?? DBNull.Value),
		new SqlParameter("@Relationship", (object)emp.Relationship ?? DBNull.Value),
		new SqlParameter("@ContactNo", (object)emp.ContactNo ?? DBNull.Value),
		new SqlParameter("@GroupID", (object)emp.GroupID ?? DBNull.Value),
		new SqlParameter("@Designation", (object)emp.Designation ?? DBNull.Value),
		new SqlParameter("@UserID", uinfo[2].Value),
		new SqlParameter("@JoiningDate", (object)emp.JoiningDate ?? DBNull.Value),
		new SqlParameter("@permanentaddress", (object)emp.permanentaddress ?? DBNull.Value),
		new SqlParameter("@remarks", (object)emp.remarks ?? DBNull.Value)
		).ToListAsync();
					EmpID = em[0].EmpID;
					emp.EmpID = EmpID;
					SaveEmpLocations(Convert.ToInt32(EmpID), emp.LocationIds);
					if (emp.photo != null)
					{
						var path = Path.Combine(_wHostEnv.WebRootPath + "/dist/img/StaffData/pics/" + EmpID + ".png");
						emp.photo.CopyTo(new FileStream(path, FileMode.Create));
					}
				}
				catch (Exception ex)
				{
					TempData["msg"] = "There was an error. " + Environment.NewLine + ex.Message;
				}
			}
			IList<StaffData> _LineMgr = null;
			IList<Departments> _Departments = null;
			var _StaffEdu = new List<StaffEdu>();
			var _staffdocs = new List<StaffDocs>();
			var _StaffWork = new List<StaffWorkHis>();
			var _Stations = new List<Stations>();
			var _Designations = new List<Designations>();
			var _JobStatus = new List<JobStatus>();
			var _UserGroups = new List<UserGroups>();
			var _Locations = new List<Locations>();
			var EmpLibrary = new List<EmpLibrary>();
			EmpLibrary empLibrary = new EmpLibrary();
			empLibrary.emp = emp;
			_LineMgr = _dContext.Database.SqlQuery<StaffData>($"usp_StaffRecord").ToList();
			empLibrary.linemgr = _LineMgr.Where(x => x.empstatus == 1).ToList();
			_Departments = _dContext.Database.SqlQuery<Departments>($"SELECT * FROM tblDepartments order by Depart").ToList();
			var lib = _dContext.Database.SqlQuery<Libs>($"SELECT * FROM tblLibraries where LiBType=1").ToList();
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
			empLibrary.departs = dd;
			_Stations = _dContext.Database.SqlQuery<Stations>($"SELECT * FROM tblStations").ToList();
			empLibrary.stations = _Stations.Where(x => x.Stat == 1).ToList();
			_Designations = _dContext.Database.SqlQuery<Designations>($"SELECT * FROM [dbo].[tblDesignations] order by Designation").ToList();
			empLibrary.desig = _Designations;
			_JobStatus = _dContext.Database.SqlQuery<JobStatus>($"SELECT  * FROM tblEmployeeStatus").ToList();
			empLibrary.jobstatus = _JobStatus;
			_UserGroups = _dContext.Database.SqlQuery<UserGroups>($"SELECT * FROM tblUserGroups where isdel=0 order by GroupName").ToList();
			empLibrary.usergroups = _UserGroups;
			_Locations = _dContext.Database.SqlQuery<Locations>($"SELECT * FROM tblLocations order by Loc").ToList();
			empLibrary.loc = _Locations.Where(x => x.LocType == 2).ToList();
			FillLocationLibrary(empLibrary, Convert.ToInt32(EmpID));
			_StaffEdu = _dContext.Database.SqlQueryRaw<StaffEdu>($"SELECT Id,Qualification, Institute, Description,EmpID FROM tblAcademicInfo where EmpID=@EmpID AND isDel=0",
				new SqlParameter("@EmpID", EmpID)).ToList();
			empLibrary.staffedus = _StaffEdu.ToList();
			_StaffWork = _dContext.Database.SqlQueryRaw<StaffWorkHis>($"usp_getStaffWorkHis @EmpID",
				new SqlParameter("@EmpID", EmpID)).ToList();
			empLibrary.workhist = _StaffWork.ToList();
			_staffdocs = _dContext.Database.SqlQueryRaw<StaffDocs>($"SELECT EmpID, Title, Type, DocName, Id FROM tblEmpDocuments WHERE (EmpID = @EmpID) AND isDel=0",
				new SqlParameter("@EmpID", EmpID)).ToList();
			empLibrary.staffdocs = _staffdocs.ToList();
			empLibrary.libs = lib.ToList();
			return View(empLibrary);
		}
		[HttpPost]
		public async Task<ActionResult> SaveWorkHisAsync(StaffWorkHis workHis)
		{
			IList<StaffWorkHis> wrk = null;
			if (ModelState.IsValid)
			{
				var uinfo = User.Claims.ToArray();
				wrk = await _dContext.Database.SqlQueryRaw<StaffWorkHis>
					($"usp_StaffWorkHis @EmpID,@UserID,@Organisation,@JobField,@Description,@DurationStart,@DurationEnd,@Id",
				new SqlParameter("@EmpID", workHis.empID),
				new SqlParameter("@UserID", uinfo[2].Value),
				new SqlParameter("@Organisation", workHis.organisation),
				new SqlParameter("@JobField", workHis.jobfield),
				new SqlParameter("@Description", (object)workHis.description ?? ""),
				new SqlParameter("@DurationStart", (object)workHis.durationstart ?? DBNull.Value),
				new SqlParameter("@DurationEnd", (object)workHis.durationend ?? DBNull.Value),
				new SqlParameter("@Id", (object)workHis.id ?? DBNull.Value)).ToListAsync();
			}
			return Json(wrk);
		}
		[HttpPost]
		public IActionResult Lists([FromBody] Dictionary<string, object> jsonData)
		{
			string s = jsonData["s"].ToString();
			string js = "{\"[Sub-Department]\":" + gM.Data2Json("SELECT Depart val,Depart txt FROM tblDepartments WHERE (ParentId IS NOT NULL)") + ",\"Designation\":" + gM.Data2Json("SELECT Designation val,Designation txt FROM tblDesignations") + ",\"EmpStatus\":[{\"val\":1,\"txt\":\"Active\"},{\"val\":0,\"txt\":\"Inactive\"}],\"Stat\":[{\"val\":\"Scheduled\",\"txt\":\"Scheduled\"},{\"val\":\"Completed\",\"txt\":\"Completed\"},{\"val\":\"Cancel\",\"txt\":\"Cancel\"}] }";
			return Ok(js);
		}
		[AuthWrite]
		public ActionResult TransferRequest()
		{
			var uinfo = User.Claims.ToArray();
			AuthRights authRights = new AuthRights();
			var chk = authRights.chk();
			var stations = _dContext.Database.SqlQuery<lstStations>($"SELECT StationID, Station FROM tblStations WHERE  (Stat = 1)").ToList();
			var emps = _dContext.Database.SqlQuery<EmpsList>($"SELECT EmpID, EmpCode, FName, LName, RManager,Designation,Station FROM tblEmployees WHERE  (EmpStatus = 1)").ToList();
			if (chk == "wr")
			{
				emps = emps.Where(x => x.RManager == int.Parse(uinfo[2].Value)).ToList();
			}
			var gRequests = _dContext.Database.SqlQueryRaw<GeneralRequests>("usp_HRGRequestsT @EmpId,@RqType",
				new SqlParameter("@EmpID", uinfo[2].Value), new SqlParameter("@RqType", "Transfer Request")).ToList();
			var generalRqst = new GeneralRqst
			{
				lstStation = stations,
				generalrequest = gRequests,
				Emplist = emps
			};
			return View(generalRqst);
		}
		[HttpPost]
		public async Task<ActionResult> TransferRequestAsync(TransferForm transferForm)
		{
			var uinfo = User.Claims.ToArray();
			AuthRights authRights = new AuthRights();
			var chk = authRights.chk();
			if (ModelState.IsValid)
			{
				var wrk = await _dContext.Database.SqlQueryRaw<EmpIds>
					($"usp_SaveHRGeneralRq @EmpId,@dtRequest,@RqType,@FrmId,@ToId,@RqDesc,@aprvstatus,@CreatedBy",
				new SqlParameter("@EmpID", transferForm.EmpID),
				new SqlParameter("@dtRequest", transferForm.dtrequest),
				new SqlParameter("@RqType", "Transfer Request"),
				new SqlParameter("@FrmId", transferForm.FrmId),
				new SqlParameter("@ToId", transferForm.ToId),
				new SqlParameter("@RqDesc", (object)transferForm.RqDesc ?? DBNull.Value),
				new SqlParameter("@aprvstatus", "To Approve"),
				new SqlParameter("@CreatedBy", uinfo[2].Value)).ToListAsync();
			}
			var stations = _dContext.Database.SqlQuery<lstStations>($"SELECT StationID, Station FROM tblStations WHERE  (Stat = 1)").ToList();
			var emps = _dContext.Database.SqlQuery<EmpsList>($"SELECT EmpID, EmpCode, FName, LName, RManager,Designation,Station FROM tblEmployees WHERE  (EmpStatus = 1)").ToList();
			if (chk == "wr")
			{
				emps = emps.Where(x => x.RManager == int.Parse(uinfo[2].Value)).ToList();
			}
			var gRequests = _dContext.Database.SqlQueryRaw<GeneralRequests>("usp_HRGRequestsT @EmpId,@RqType",
				new SqlParameter("@EmpID", uinfo[2].Value), new SqlParameter("@RqType", "Transfer Request")).ToList();
			var generalRqst = new GeneralRqst
			{
				lstStation = stations,
				generalrequest = gRequests,
				Emplist = emps
			};
			return View(generalRqst);
		}
		[AuthWrite]
		public ActionResult PromotionRequest()
		{
			var uinfo = User.Claims.ToArray();
			AuthRights authRights = new AuthRights();
			var chk = authRights.chk();
			var stations = _dContext.Database.SqlQuery<lstStations>($"SELECT StationID, Station FROM tblStations WHERE  (Stat = 1)").ToList();
			var _Designations = _dContext.Database.SqlQuery<Designations>($"SELECT * FROM [dbo].[tblDesignations] order by Designation").ToList();
			var emps = _dContext.Database.SqlQuery<ddEmps>($"SELECT EmpID, EmpCode, FName, LName, RManager,Designation FROM tblEmployees WHERE  (EmpStatus = 1)").ToList();
			if (chk == "wr")
			{
				emps = emps.Where(x => x.RManager == int.Parse(uinfo[2].Value)).ToList();
			}
			var gRequests = _dContext.Database.SqlQueryRaw<GeneralRequests>("usp_HRGRequestsP @EmpId,@RqType",
				new SqlParameter("@EmpID", uinfo[2].Value), new SqlParameter("@RqType", "Promotion Request")).ToList();
			var generalRqst = new GeneralRqst
			{
				lstStation = stations,
				generalrequest = gRequests,
				ddEmplist = emps,
				designations = _Designations
			};
			return View(generalRqst);
		}
		[HttpPost]
		public async Task<ActionResult> PromotionRequest(TransferForm transferForm)
		{
			var uinfo = User.Claims.ToArray();
			AuthRights authRights = new AuthRights();
			var chk = authRights.chk();
			if (ModelState.IsValid)
			{
				var wrk = await _dContext.Database.SqlQueryRaw<EmpIds>
					($"usp_SaveHRGeneralRq @EmpId,@dtRequest,@RqType,@FrmId,@ToId,@RqDesc,@aprvstatus,@CreatedBy",
				new SqlParameter("@EmpID", transferForm.EmpID),
				new SqlParameter("@dtRequest", transferForm.dtrequest),
				new SqlParameter("@RqType", "Promotion Request"),
				new SqlParameter("@FrmId", transferForm.FrmId),
				new SqlParameter("@ToId", transferForm.ToId),
				new SqlParameter("@RqDesc", (object)transferForm.RqDesc ?? DBNull.Value),
				new SqlParameter("@aprvstatus", "To Approve"),
				new SqlParameter("@CreatedBy", uinfo[2].Value)).ToListAsync();
			}
			var stations = _dContext.Database.SqlQuery<lstStations>($"SELECT StationID, Station FROM tblStations WHERE  (Stat = 1)").ToList();
			var emps = _dContext.Database.SqlQuery<ddEmps>($"SELECT EmpID, EmpCode, FName, LName, RManager,Designation FROM tblEmployees WHERE  (EmpStatus = 1)").ToList();
			var _Designations = _dContext.Database.SqlQuery<Designations>($"SELECT * FROM [dbo].[tblDesignations] order by Designation").ToList();
			if (chk == "wr")
			{
				emps = emps.Where(x => x.RManager == int.Parse(uinfo[2].Value)).ToList();
			}
			var gRequests = _dContext.Database.SqlQueryRaw<GeneralRequests>("usp_HRGRequestsP @EmpId,@RqType",
				new SqlParameter("@EmpID", uinfo[2].Value), new SqlParameter("@RqType", "Promotion Request")).ToList();
			var generalRqst = new GeneralRqst
			{
				lstStation = stations,
				generalrequest = gRequests,
				ddEmplist = emps,
				designations = _Designations
			};
			return View(generalRqst);
		}
		[AuthWrite]
		public ActionResult HRRequestApprovals()
		{
			var uinfo = User.Claims.ToArray();
			AuthRights authRights = new AuthRights();
			var chk = authRights.chk();
			var gRequests = _dContext.Database.SqlQueryRaw<GeneralRequests>("usp_HRGRequestsAprv @EmpId,@auth",
				new SqlParameter("@EmpID", uinfo[2].Value), new SqlParameter("@auth", chk)).ToList();
			var exptRequests = _dContext.Database.SqlQueryRaw<ExcptRequests>("usp_ExemptionRequestsAprovel @EmpId,@auth",
	new SqlParameter("@EmpID", uinfo[2].Value), new SqlParameter("@auth", chk)).ToList();
			var generalRqst = new GeneralRqst
			{
				generalrequest = gRequests,
				excptRequests = exptRequests
			};
			return View(generalRqst);
		}
		[HttpGet]
		public ActionResult Refuse(int Id)
		{
			var uinfo = User.Claims.ToArray();
			var em = _dContext.Database.SqlQueryRaw<EmpIds>($"usp_RequestRefuse @EmpId,@Id",
				new SqlParameter("@EmpID", uinfo[2].Value),
				new SqlParameter("@Id", Id)).ToList();
			return RedirectToAction("HRRequestApprovals", "HR");
		}
		[HttpGet]
		public ActionResult Approve(int Id)
		{
			var uinfo = User.Claims.ToArray();
			var em = _dContext.Database.SqlQueryRaw<EmpIds>($"usp_RequestApprove @EmpId,@Id",
				new SqlParameter("@EmpID", uinfo[2].Value),
				new SqlParameter("@Id", Id)).ToList();
			return RedirectToAction("HRRequestApprovals", "HR");
		}
		[HttpGet]
		public ActionResult ExpRefuse(int Id)
		{
			var uinfo = User.Claims.ToArray();
			gM.FillDSet("UPDATE tblExemptionRequests SET aprvBy = " + uinfo[2].Value + ", dtaprv = GETDATE(), aprvstatus = 'Refused' WHERE  (Id = " + Id + ")");
			return RedirectToAction("HRRequestApprovals", "HR");
		}
		[HttpGet]
		public ActionResult ExpApprove(int Id)
		{
			var uinfo = User.Claims.ToArray();
			gM.FillDSet("UPDATE tblExemptionRequests SET aprvBy = " + uinfo[2].Value + ", dtaprv = GETDATE(), aprvstatus = 'Approved' WHERE  (Id = " + Id + ")");
			return RedirectToAction("HRRequestApprovals", "HR");
		}
		[HttpPost]
		[HttpGet]
		public async Task<ActionResult> StaffProfileAsync(Empolyee emp, int id)
		{
			AuthRights authRights = new AuthRights();
			var chk = authRights.chk();
			var uinfo = User.Claims.ToArray();
			if (chk == "" && uinfo[2].Value != id.ToString())
			{
				return RedirectToAction("", "");
			}
			var EmpID = id;
			if (emp == null || emp.EmpID == 0)
			{
				emp = _dContext.Database.SqlQueryRaw<Empolyee>($"select EmpID,EmpCode, FName, LName, FatherName, MobileNo, Email, Marital, Gender, DateOfBirth, Address,  ContactPerson,Relationship, ContactNo, GroupID, Designation, CreatedBy, JoiningDate, EmpStatus,permanentaddress, remarks from tblEmployees where EmpID=@EmpID",
				new SqlParameter("@EmpID", EmpID)).First();
			}
			//if (ModelState["FName"].ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid)
			if (ModelState.IsValid)
			{
				try
				{
					uinfo = User.Claims.ToArray();
					var em = await _dContext.Database.SqlQueryRaw<EmpIds>
							($"usp_SaveEmp @EmpID,@EmpCode,@FName,@LName,@FatherName,@MobileNo,@Email,@Marital,@Gender,@DateOfBirth,@Address,@ContactPerson,@Relationship,@ContactNo,@GroupID,@Designation,@UserID,@JoiningDate,@permanentaddress,@remarks",
		new SqlParameter("@EmpID", emp.EmpID),
		new SqlParameter("@EmpCode", emp.EmpCode),
		new SqlParameter("@FName", emp.FName),
		new SqlParameter("@LName", (object)emp.LName ?? DBNull.Value),
		new SqlParameter("@FatherName", (object)emp.FatherName ?? DBNull.Value),
		new SqlParameter("@MobileNo", (object)emp.MobileNo ?? DBNull.Value),
		new SqlParameter("@Email", (object)emp.Email ?? DBNull.Value),
		new SqlParameter("@Marital", emp.Marital),
		new SqlParameter("@Gender", emp.Gender),
		new SqlParameter("@DateOfBirth", (object)emp.DateOfBirth ?? DBNull.Value),
		new SqlParameter("@Address", (object)emp.Address ?? DBNull.Value),
		new SqlParameter("@ContactPerson", (object)emp.ContactPerson ?? DBNull.Value),
		new SqlParameter("@Relationship", (object)emp.Relationship ?? DBNull.Value),
		new SqlParameter("@ContactNo", (object)emp.ContactNo ?? DBNull.Value),
		new SqlParameter("@GroupID", (object)emp.GroupID ?? DBNull.Value),
		new SqlParameter("@Designation", (object)emp.Designation ?? DBNull.Value),
		new SqlParameter("@UserID", uinfo[2].Value),
		new SqlParameter("@JoiningDate", (object)emp.JoiningDate ?? DBNull.Value),
		new SqlParameter("@permanentaddress", (object)emp.permanentaddress ?? DBNull.Value),
		new SqlParameter("@remarks", (object)emp.remarks ?? DBNull.Value)).ToListAsync();
					EmpID = em[0].EmpID;
					emp.EmpID = EmpID;
					if (emp.photo != null)
					{
						var path = Path.Combine(_wHostEnv.WebRootPath + "/dist/img/StaffData/pics/" + EmpID + ".png");
						emp.photo.CopyTo(new FileStream(path, FileMode.Create));
					}
				}
				catch (Exception ex)
				{
					TempData["msg"] = "There was an error. " + Environment.NewLine + ex.Message;
				}
			}
			IList<StaffData> _LineMgr = null;
			IList<Departments> _Departments = null;
			var _StaffEdu = new List<StaffEdu>();
			var _staffdocs = new List<StaffDocs>();
			var _StaffWork = new List<StaffWorkHis>();
			var _Stations = new List<Stations>();
			var _Designations = new List<Designations>();
			var _JobStatus = new List<JobStatus>();
			var _UserGroups = new List<UserGroups>();
			var _Locations = new List<Locations>();
			var EmpLibrary = new List<EmpLibrary>();
			EmpLibrary empLibrary = new EmpLibrary();
			empLibrary.emp = emp;
			_LineMgr = _dContext.Database.SqlQuery<StaffData>($"usp_StaffRecord").ToList();
			empLibrary.linemgr = _LineMgr.Where(x => x.empstatus == 1).ToList();
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
			empLibrary.departs = dd;
			_Stations = _dContext.Database.SqlQuery<Stations>($"SELECT * FROM tblStations").ToList();
			empLibrary.stations = _Stations.Where(x => x.Stat == 1).ToList();
			_Designations = _dContext.Database.SqlQuery<Designations>($"SELECT * FROM [dbo].[tblDesignations] order by Designation").ToList();
			empLibrary.desig = _Designations;
			_JobStatus = _dContext.Database.SqlQuery<JobStatus>($"SELECT  * FROM tblEmployeeStatus").ToList();
			empLibrary.jobstatus = _JobStatus;
			_UserGroups = _dContext.Database.SqlQuery<UserGroups>($"SELECT * FROM tblUserGroups order by GroupName").ToList();
			empLibrary.usergroups = _UserGroups;
			_Locations = _dContext.Database.SqlQuery<Locations>($"SELECT * FROM tblLocations order by Loc").ToList();
			empLibrary.loc = _Locations.Where(x => x.LocType == 2).ToList();
			_StaffEdu = _dContext.Database.SqlQueryRaw<StaffEdu>($"SELECT Id,Qualification, Institute, Description,EmpID FROM tblAcademicInfo where EmpID=@EmpID AND isDel=0",
				new SqlParameter("@EmpID", EmpID)).ToList();
			empLibrary.staffedus = _StaffEdu.ToList();
			_StaffWork = _dContext.Database.SqlQueryRaw<StaffWorkHis>($"usp_getStaffWorkHis @EmpID",
				new SqlParameter("@EmpID", EmpID)).ToList();
			empLibrary.workhist = _StaffWork.ToList();
			_staffdocs = _dContext.Database.SqlQueryRaw<StaffDocs>($"SELECT EmpID, Title, Type, DocName, Id FROM tblEmpDocuments WHERE (EmpID = @EmpID) AND isDel=0",
				new SqlParameter("@EmpID", EmpID)).ToList();
			empLibrary.staffdocs = _staffdocs.ToList();
			return View(empLibrary);
		}
		[AuthWrite]
		public ActionResult OrgChart()
		{
			var uinfo = User.Claims.ToArray();
			var em = _dContext.Database.SqlQueryRaw<orgchart>($"SELECT tblEmployees.EmpID AS id, tblEmployees.FName + ' ' + ISNULL(tblEmployees.LName, '') AS name, isnull(tblDesignations.Designation,'') AS title, isnull(tblEmployees.RManager,0) AS pid, tblEmployees.Email AS email,'/dist/img/StaffData/pics/' + CONVERT(varchar, tblEmployees.EmpID) + '.png' AS img FROM tblEmployees LEFT OUTER JOIN tblDesignations ON tblEmployees.Designation = tblDesignations.Id WHERE  (tblEmployees.EmpStatus = 1)").ToList();
			return View(em);
		}
		[HttpPost]
		public ActionResult SetEPass(string pass, string rpass, string empid)
		{
			if (IsPasswordStrong(pass))
			{
				if (pass == rpass && pass != "undefined" && pass != " " && pass != null && pass != "")
				{
					var uinfo = User.Claims.ToArray();
					var epass = gM.HashPasword(pass);
					var em = _dContext.Database.SqlQueryRaw<EmpIds>($"UPDATE tblEmployees SET Password = @pass WHERE (EmpID = @EmpID)",
						new SqlParameter("@EmpID", empid),
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
