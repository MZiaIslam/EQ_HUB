using EHUB.Models.Administration;
using EHUB.Models.Attendance;
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
using System.Web;
using System.Xml;
namespace EHUB.Controllers.HRManagement
{
    [AuthUser]
   
    public class LeavesController : Controller
    {
        private readonly DataContext _dContext;
        private readonly IWebHostEnvironment _wHostEnv;
        private readonly GM _gm;
        AuthRights authRights = new AuthRights();
        static string spEx = "";
        GM gM = new GM();
        public LeavesController(DataContext dContext, IWebHostEnvironment wHostEnv, GM gm)
        {
            _dContext = dContext;
            _wHostEnv = wHostEnv;
            _gm = gm;
        }
        [AuthWrite]
        public ActionResult LeaveApplication()
        {
            var uinfo = User.Claims.ToArray();
            var leaveTypes = _dContext.Database.SqlQueryRaw<LeaveType>($"usp_Leavetypes @EmpId",
                new SqlParameter("@EmpID", uinfo[2].Value)).AsEnumerable().Where(x => x.balance > 0).ToList();
            var leaveRequests = _dContext.Database.SqlQueryRaw<LeaveRequests>("usp_LeaveRequests @EmpId",
                new SqlParameter("@EmpID", uinfo[2].Value)).ToList();
            var leaveSumm = _dContext.Database.SqlQueryRaw<LeaveSumm>("usp_LeaveStatus @EmpId",
               new SqlParameter("@EmpID", uinfo[2].Value)).ToList();
            var leaveLibrary = new LeaveLibrary
            {
                leaveType = leaveTypes,
                leaves = leaveRequests,
                lvSumm = leaveSumm
            };
            return View(leaveLibrary);
        }
        [HttpPost]
        public async Task<ActionResult> LeaveApplication(Leaves leaves)
        {
            var uinfo = User.Claims.ToArray();
            if (ModelState.IsValid)
            {
                try
                {
                    var lv = await _dContext.Database.SqlQueryRaw<LvIds>(
                        "usp_SaveLVRequest @EmpId, @LeaveID, @dtFrom, @dtTo, @Ldesc, @attachment, @UserID, @LvId, @lvduration",
                        new SqlParameter("@LeaveID", leaves.LeaveID),
                        new SqlParameter("@EmpID", uinfo[2].Value),
                        new SqlParameter("@dtFrom", leaves.dtFrom),
                        new SqlParameter("@dtTo", leaves.dtTo),
                        new SqlParameter("@Ldesc", (object)leaves.Ldesc ?? DBNull.Value),
                        new SqlParameter("@attachment", leaves.doc?.FileName ?? (object)DBNull.Value),
                        new SqlParameter("@UserID", uinfo[2].Value),
                        new SqlParameter("@LvId", (object)leaves.LvId ?? DBNull.Value),
                        new SqlParameter("@lvduration", leaves.lvduration)
                    ).ToListAsync();
                    var lvId = lv.FirstOrDefault()?.LvId;
                    if (leaves.doc != null && lvId != null)
                    {
                        var directoryPath = Path.Combine(_wHostEnv.WebRootPath, "dist", "img", "StaffData", "leaves", lvId.ToString());
                        if (!Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(directoryPath);
                        }
                        var filePath = Path.Combine(directoryPath, leaves.doc.FileName);
                        using (var fileStream = new FileStream(filePath, FileMode.CreateNew))
                        {
                            await leaves.doc.CopyToAsync(fileStream);
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["msg"] = "There was an error: " + ex.Message;
                }
            }
            //var leaveTypes = await _dContext.Database.SqlQuery<LeaveType>($"SELECT LeaveID, Leave FROM tblLeavetypes").ToListAsync();
            //var leaveRequests = await _dContext.Database.SqlQueryRaw<LeaveRequests>("usp_LeaveRequests @EmpId", new SqlParameter("@EmpID", uinfo[2].Value)).ToListAsync();
            //var leaveLibrary = new LeaveLibrary
            //{
            //    leaveType = leaveTypes,
            //    leaves = leaveRequests
            //};
            //return View(leaveLibrary);
            var leaveTypes = _dContext.Database.SqlQueryRaw<LeaveType>($"usp_Leavetypes @EmpId",
             new SqlParameter("@EmpID", uinfo[2].Value)).AsEnumerable().Where(x => x.balance > 0).ToList();
            var leaveRequests = _dContext.Database.SqlQueryRaw<LeaveRequests>("usp_LeaveRequests @EmpId",
                new SqlParameter("@EmpID", uinfo[2].Value)).ToList();
            var leaveSumm = _dContext.Database.SqlQueryRaw<LeaveSumm>("usp_LeaveStatus @EmpId",
               new SqlParameter("@EmpID", uinfo[2].Value)).ToList();
            var leaveLibrary = new LeaveLibrary
            {
                leaveType = leaveTypes,
                leaves = leaveRequests,
                lvSumm = leaveSumm
            };
            return View(leaveLibrary);
        }
        [AuthWrite]
        public ActionResult StaffLeaves()
        {
            var uinfo = User.Claims.ToArray();
            var _leavetype = new List<LeaveType>();
             _leavetype = _dContext.Database.SqlQueryRaw<LeaveType>($"usp_Leavetypes @EmpId",
         new SqlParameter("@EmpID", uinfo[2].Value)).ToList();
          
            var _leaves = new List<LeaveRequests>();
            _leaves = _dContext.Database.SqlQueryRaw<LeaveRequests>($"usp_LeaveRequestsAll @EmpId",
                new SqlParameter("@EmpID", uinfo[2].Value)).ToList();
            LeaveLibrary _leaverequest = new LeaveLibrary();
            IList<StaffData> _staff = null;
            _leaverequest.leaveType = _leavetype;
            _leaverequest.leaves = _leaves;
            _staff = _dContext.Database.SqlQuery<StaffData>($"usp_StaffRecord").ToList();
            var chk = authRights.chk();
            var mgr = authRights.mgr();
            if (mgr != 0 && chk != "f")
            {
                _staff = _staff.Where(x => x.RManager.ToString() == uinfo[2].Value || x.empid.ToString() == uinfo[2].Value).ToList();
            }
            _leaverequest.staff = _staff.Where(x => x.empstatus == 1).ToList();
            return View(_leaverequest);
        }
        [HttpPost]
        public async Task<ActionResult> StaffLeaves(Leaves leaves)
        {
            var uinfo = User.Claims.ToArray();
            if (ModelState.IsValid)
            {
                try
                {
                    var lv = await _dContext.Database.SqlQueryRaw<LvIds>
                            ($"usp_SaveLVRequest @EmpId,@LeaveID,@dtFrom,@dtTo,@Ldesc,@attachment,@UserID,@LvId,@lvduration",
                        new SqlParameter("@LeaveID", leaves.LeaveID),
                            new SqlParameter("@EmpID", leaves.EmpId),
                            new SqlParameter("@dtFrom", leaves.dtFrom),
                            new SqlParameter("@dtTo", leaves.dtTo),
                            new SqlParameter("@Ldesc", (object)leaves.Ldesc ?? DBNull.Value),
                            new SqlParameter("@attachment", leaves.doc != null ? (object)leaves.doc.FileName ?? DBNull.Value : DBNull.Value),
                        new SqlParameter("@UserID", uinfo[2].Value),
                        new SqlParameter("@LvId", (object)leaves.LvId ?? DBNull.Value),
new SqlParameter("@lvduration", leaves.lvduration)
                            ).ToListAsync();
                    var LvId = lv[0].LvId;
                    if (leaves.doc != null)
                    {
                        bool exists = System.IO.Directory.Exists(_wHostEnv.WebRootPath + "/dist/img/StaffData/leaves/" + LvId);
                        if (!exists)
                            System.IO.Directory.CreateDirectory(_wHostEnv.WebRootPath + "/dist/img/StaffData/leaves/" + LvId);
                        var path = Path.Combine(_wHostEnv.WebRootPath + "/dist/img/StaffData/leaves/" + LvId + "/" + leaves.doc.FileName);
                        leaves.doc.CopyTo(new FileStream(path, FileMode.CreateNew));
                    }
                }
                catch (Exception ex)
                {
                    TempData["msg"] = "There was an error. " + Environment.NewLine + ex.Message;
                }
            }

            return RedirectToAction("StaffLeaves", "Leaves");
        }
        [HttpPost]
        public ActionResult RequestApprovals(Fillterdata fillterdata)
        {
            var uinfo = User.Claims.ToArray();
            var _Stations = _dContext.Database.SqlQuery<Stations>($"SELECT * FROM tblStations where stat=1").Select(p => new SelectListItem
            {
                Value = p.StationID.ToString(),
                Text = p.Station,
                Selected = p.StationID.ToString() == fillterdata.station ? true : false
            })
            .ToList();

            var _emps = _dContext.Database.SqlQuery<StaffData>($"usp_StaffRecord").AsEnumerable().Select(p => new SelectListItem
            {
                Value = p.empid.ToString(),
                Text = p.empcode.ToString() + " " + p.empname,
                Selected = p.empid.ToString() == fillterdata.emp ? true : false
            }).ToList();





            var _subdepart = _dContext.Database.SqlQuery<Departments>($"SELECT * FROM tblDepartments where ParentId is not NULL").Select(p => new SelectListItem
            {
                Value = p.DepartID.ToString(),
                Text = p.Depart,
                Selected = p.DepartID.ToString() == fillterdata.subdepart ? true : false
            }).ToList();
            var _depart = _dContext.Database.SqlQuery<Departments>($"SELECT * FROM tblDepartments where ParentId is NULL").Select(p => new SelectListItem
            {
                Value = p.DepartID.ToString(),
                Text = p.Depart,
                Selected = p.DepartID.ToString() == fillterdata.depart ? true : false
            }).ToList();
            var _leavetype = _dContext.Database.SqlQueryRaw<LeaveType>($"usp_Leavetypes @EmpId",
             new SqlParameter("@EmpID", uinfo[2].Value)).AsEnumerable().Select(p => new SelectListItem
             {
                 Value = p.LeaveID.ToString(),
                 Text = p.Leave,
                 Selected = p.LeaveID.ToString() == fillterdata.LeaveType ? true : false
             }).ToList();



            var _staffdata = new List<StaffData>();
            if (ModelState.IsValid)
            {
                //_staffdata = _dContext.Database.SqlQuery<StaffData>($"usp_StaffRecord").ToList();
            }
            var _leaves = new List<LeaveRequests>();
            _leaves = _dContext.Database.SqlQueryRaw<LeaveRequests>($"usp_LeavePendingApprovals @EmpId,@aprvstatus",
                new SqlParameter("@EmpID", uinfo[2].Value),
                new SqlParameter("@aprvstatus", fillterdata.LeaveStatus)).ToList();
            if (fillterdata.station != "0")
            {
                _leaves = _leaves.Where(x => x.station.ToString() == fillterdata.station).ToList();
            }

            if (fillterdata.LeaveType != "0")
            {
                _leaves = _leaves.Where(x => x.leaveID.ToString() == fillterdata.LeaveType).ToList();
            }
            if (fillterdata.emp != "0")
            {
                _leaves = _leaves.Where(x => x.empid.ToString() == fillterdata.emp).ToList();
            }

            if (fillterdata.subdepart != "0")
            {
                _leaves = _leaves.Where(x => x.subDepartment.ToString() == fillterdata.subdepart).ToList();
            }
            if (fillterdata.chkdt == "on")
            {
                _leaves = _leaves.Where(x => x.dtFrom <= fillterdata.todate && x.dtTo >= fillterdata.fromdate).ToList();
            }
            var fillters = new fillters();
            fillters.stations = _Stations;
            fillters.emps = _emps;
    
            fillters.depart = _depart;
            fillters.subdepart = _subdepart;
            fillters.leaves = _leaves;
            fillters.linemgr = _staffdata;
            fillters.leavetypes = _leavetype;
            fillters.fills = fillterdata;
            return View(fillters);











            //var _leavetype = new List<LeaveType>();

            //_leavetype = _dContext.Database.SqlQueryRaw<LeaveType>($"usp_Leavetypes @EmpId",
            // new SqlParameter("@EmpID", uinfo[2].Value)).AsEnumerable().Where(x => x.balance > 0).ToList();

            //LeaveLibrary _leaverequest = new LeaveLibrary();
            //IList<StaffData> _staff = null;
            //_leaverequest.leaveType = _leavetype;
            //_leaverequest.leaves = _leaves;
            //_staff = _dContext.Database.SqlQuery<StaffData>($"usp_StaffRecord").ToList();
            //_leaverequest.staff = _staff.Where(x => x.empstatus == 1).ToList();
            //return View(_leaverequest);
        }
        [AuthWrite]
        public ActionResult RequestApprovals()
        {
            var uinfo = User.Claims.ToArray();
            var _Stations = _dContext.Database.SqlQuery<Stations>($"SELECT * FROM tblStations where stat=1").Select(p => new SelectListItem
            {
                Value = p.StationID.ToString(),
                Text = p.Station
            })
            .ToList();

            var _emps = _dContext.Database.SqlQuery<StaffData>($"usp_StaffRecord").AsEnumerable().Select(p => new SelectListItem
            {
                Value = p.empid.ToString(),
                Text = p.empcode.ToString() + " " + p.empname
            }).ToList();

            var _JobStatus = _dContext.Database.SqlQuery<JobStatus>($"SELECT  * FROM tblEmployeeStatus").Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.EmployeeStatus
            }).ToList();



            var _subdepart = _dContext.Database.SqlQuery<Departments>($"SELECT * FROM tblDepartments where ParentId is not NULL").Select(p => new SelectListItem
            {
                Value = p.DepartID.ToString(),
                Text = p.Depart
            }).ToList();
            var _depart = _dContext.Database.SqlQuery<Departments>($"SELECT * FROM tblDepartments where ParentId is NULL").Select(p => new SelectListItem
            {
                Value = p.DepartID.ToString(),
                Text = p.Depart
            }).ToList();
            var _leavetype = _dContext.Database.SqlQueryRaw<LeaveType>($"usp_Leavetypes @EmpId",
             new SqlParameter("@EmpID", uinfo[2].Value)).AsEnumerable().Select(p => new SelectListItem
             {
                 Value = p.LeaveID.ToString(),
                 Text = p.Leave
             }).ToList();
            var _staffdata = new List<StaffData>();
            if (ModelState.IsValid)
            {
                //_staffdata = _dContext.Database.SqlQuery<StaffData>($"usp_StaffRecord").ToList();
            }
            var _leaves = new List<LeaveRequests>();

            _leaves = _dContext.Database.SqlQueryRaw<LeaveRequests>($"usp_LeavePendingApprovals @EmpId,@aprvstatus",
              new SqlParameter("@EmpID", uinfo[2].Value),
              new SqlParameter("@aprvstatus", "To Approve")).ToList();

            var fillters = new fillters();
            fillters.stations = _Stations;
            fillters.emps = _emps;
            fillters.jobstat = _JobStatus;
            fillters.depart = _depart;
            fillters.subdepart = _subdepart;
            fillters.leaves = _leaves;
            fillters.linemgr = _staffdata;
            fillters.leavetypes = _leavetype;
            return View(fillters);











            //var _leavetype = new List<LeaveType>();
           
            //_leavetype = _dContext.Database.SqlQueryRaw<LeaveType>($"usp_Leavetypes @EmpId",
            // new SqlParameter("@EmpID", uinfo[2].Value)).AsEnumerable().Where(x => x.balance > 0).ToList();
       
            //LeaveLibrary _leaverequest = new LeaveLibrary();
            //IList<StaffData> _staff = null;
            //_leaverequest.leaveType = _leavetype;
            //_leaverequest.leaves = _leaves;
            //_staff = _dContext.Database.SqlQuery<StaffData>($"usp_StaffRecord").ToList();
            //_leaverequest.staff = _staff.Where(x => x.empstatus == 1).ToList();
            //return View(_leaverequest);
        }
        [HttpGet]
        public ActionResult Refuse(int Id,string remarks)
        {
            var uinfo = User.Claims.ToArray();

            var _leaves = _dContext.Database.SqlQueryRaw<EmpIds>($"usp_LeaveRefuse @EmpId,@Id,@remarks",
                new SqlParameter("@EmpID", uinfo[2].Value),
                new SqlParameter("@Id", Id),
                new SqlParameter("@remarks", remarks)).ToList();
            
            return RedirectToAction("RequestApprovals", "Leaves");
        }
        [HttpGet]
        public ActionResult Approve(int Id,string remarks)
        {
            var uinfo = User.Claims.ToArray();

 
            
            var _leaves = _dContext.Database.SqlQueryRaw<EmpIds>($"usp_LeaveApprove @EmpId,@Id,@remarks",
                new SqlParameter("@EmpID", uinfo[2].Value),
                new SqlParameter("@Id", Id),
                new SqlParameter("@remarks", remarks)).ToList();
   
            return RedirectToAction("RequestApprovals", "Leaves");
        }
        [AuthWrite]
        public ActionResult LeaveReport()
        {
            return View();
        }
        [HttpPost]
        public IActionResult ReportDTLs([FromBody] Dictionary<string, object> jsonData)
        {
            string RptID = jsonData["RptID"].ToString();
            string width = jsonData["width"].ToString();
            string xy = jsonData["xy"].ToString();
            string ctyp = jsonData["ctyp"].ToString();
            string responseMessage = "¦Q1|" + xy + "| ||0|" + xy + "|0|0|y|" + ctyp + "|1|1|300|" + width + "";
            return Ok(responseMessage);
        }

        [HttpPost]
        public IActionResult ReportData([FromBody] Dictionary<string, object> jsonData)
        {
            string sp = jsonData["sp"]?.ToString();
            string groups = jsonData["Grps"]?.ToString();
            string columns = jsonData["Columns"]?.ToString();
            string wheres = jsonData["Wheres"]?.ToString();
            string xy = jsonData["xy"]?.ToString();
            string id = jsonData["ID"]?.ToString();
            // Decode and sanitize input
            string spEx = $"{sp} '{HttpUtility.UrlDecode(groups)}', '{HttpUtility.UrlDecode(columns)}', '{SanitizeWhereClause(HttpUtility.UrlDecode(wheres))}'";
            if (xy == "1")
            {
                return Ok(gM.HTMLshTable(spEx, "chrt-tb" + id, "table table-hover table-striped table-sm"));
            }
            else
            {
                return Ok(gM.HTMLchTable($"{spEx} {xy}", "chrt-tb" + id, "table table-hover table-striped table-sm"));
            }
        }
        private string SanitizeWhereClause(string whereClause)
        {
            // Sanitize the WHERE clause to prevent SQL injection
            return whereClause.Replace("''''", "''")
                              .Replace("lk''", " like '")
                              .Replace("''", "'")
                              .Replace("'", "''");
        }
        [HttpPost]
        public IActionResult Lists([FromBody] Dictionary<string, object> jsonData)
        {
            string s = jsonData["s"].ToString();
            string js = "{\"lvcat\": [{ \"txt\": \"Full Day\" }, { \"txt\": \"Half Day\" },{ \"txt\": \"Quarter Day\" }], \"Leave\":" + gM.Data2Json("SELECT Leave val,Leave txt FROM tblLeavetypes") + ",\"Depart\":" + gM.Data2Json("SELECT Depart val,Depart txt FROM tblDepartments WHERE (ParentId IS NOT NULL)") + ",\"Designation\":" + gM.Data2Json("SELECT Designation val,Designation txt FROM tblDesignations") + " }";
            return Ok(js);
        }
        public IActionResult SaveReport(string info)
        {
            var uinfo = User.Claims.ToArray();
            string[] dataID = info.Split(':');
            gM.FillDSet("sp_CreateReport N'" + dataID[0] + "','" + dataID[1] + "','" + dataID[2] + "','" + dataID[3] + "','" + uinfo[2].Value + "'");
            return Ok();
        }
        public IActionResult SaveSection(string info)
        {
            gM.FillDSet("INSERT INTO tblReportDTL(sp_sql, gruops, sums, remarks, remarksPos, Wheres, tableview, tablePos, row_col, chattype, sec_sort, RptID,chrtview) VALUES (" + info);
            return Ok();
        }
        public IActionResult ReportList()
        {
            var uinfo = User.Claims.ToArray();
            string res = gM.FillDSet("select dbo.fn_RptList(" + uinfo[2].Value + ")").Tables[0].Rows[0][0].ToString();
            return Ok(res);
        }
        [AuthWrite]
        public ActionResult LeaveQuota()
        {
            var uinfo = User.Claims.ToArray();
           
            var _leavtype = _dContext.Database.SqlQueryRaw<LeaveType>($"usp_Leavetypes @EmpId",
              new SqlParameter("@EmpID", uinfo[2].Value)).ToList();
            var _Departments = _dContext.Database.SqlQuery<Departments>($"SELECT * FROM tblDepartments order by Depart").ToList();
            var treedepart = (from d in _Departments
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
            Quota _pgquota = new Quota();
            _pgquota.departs = treedepart;
            _pgquota.leavetyps = _leavtype;
            var _staff = _dContext.Database.SqlQuery<StaffData>($"usp_StaffRecord").ToList();
            _pgquota.staff = _staff.Where(x => x.empstatus == 1).ToList();
            return View(_pgquota);
        }
        [HttpPost]
        public async Task<ActionResult> LeaveQuota(LvQuota quota)
        {
            var uinfo = User.Claims.ToArray();
            if (ModelState.IsValid)
            {
                try
                {
                    string Ids;
                    if (quota.mode == 1)
                    {
                        Ids = string.Join(",", quota.DepartIds);
                    }
                    else { Ids = string.Join(",", quota.EmpIds); }
                    var lv = await _dContext.Database.SqlQueryRaw<LvIds>
                            ($"usp_SaveQuota @Mode,@Ids ,@LeaveID,@Allowed,@yrQuota,@UserID",
                        new SqlParameter("@Mode", quota.mode),
                            new SqlParameter("@Ids", Ids),
        
                            new SqlParameter("@LeaveID", quota.LeaveID),
                            new SqlParameter("@Allowed", quota.Allowed),
                            new SqlParameter("@yrQuota", quota.yrQuota),
                        new SqlParameter("@UserID", uinfo[2].Value)
                            ).ToListAsync();
                    var LvId = lv[0].LvId;
                }
                catch (Exception ex)
                {
                    TempData["msg"] = "There was an error. " + Environment.NewLine + ex.Message;
                }
            }
            return RedirectToAction("LeaveQuota", "Leaves");
        }
        [HttpGet]
        public async Task<ActionResult> GetQuota(int id)
        {
            IList<QuotaInfo> Quo = null;
            if (ModelState.IsValid)
            {
                var uinfo = User.Claims.ToArray();
                Quo = await _dContext.Database.SqlQueryRaw<QuotaInfo>
                    ($"usp_LvQuotaInfo @EmpId",
                new SqlParameter("@EmpId", id)).ToListAsync();
            }
            return Json(Quo);
        }
        [HttpGet]
        public async Task<ActionResult> DelQuotaAsync(string id)
        {
            IList<QuotaInfo> edu = null;
            if (ModelState.IsValid)
            {
                var uinfo = User.Claims.ToArray();
                edu = await _dContext.Database.SqlQueryRaw<QuotaInfo>
                    ($"usp_QuotaDel @Id,@UserID",
                new SqlParameter("@UserID", uinfo[2].Value),
                new SqlParameter("@id", id)).ToListAsync();
            }
            return Json(edu);
        }
        [HttpGet]
        public async Task<ActionResult> UpdateQuotaAsync(string id,string txt)
        {
            IList<QuotaInfo> edu = null;
            if (ModelState.IsValid)
            {
                var uinfo = User.Claims.ToArray();
                edu = await _dContext.Database.SqlQueryRaw<QuotaInfo>
                    ($"usp_QuotaUpdate @Id,@UserID,@val",
                new SqlParameter("@UserID", uinfo[2].Value),
                new SqlParameter("@id", id),
                new SqlParameter("@val", txt)).ToListAsync();
            }
            return Json(edu);
        }
        [HttpGet]
        public ActionResult DelReq(int Id)
        {
            var uinfo = User.Claims.ToArray();

            gM.FillDSet($"UPDATE tblLeaves SET DelBy = {uinfo[2].Value}, dtDel = GETDATE(), isDel = 1, aprvstatus = 'To Approve' WHERE  (Id = {Id})");

            return RedirectToAction("LeaveApplication", "Leaves");
        }
        [HttpGet]
        public async Task<ActionResult> leaveTypes(int id)
        {
            
                var leaveTypes =_dContext.Database.SqlQueryRaw<LeaveType>($"usp_Leavetypes @EmpId",
                    new SqlParameter("@EmpID", id)).AsEnumerable().Where(x => x.balance > 0).ToList();
           
            return Json(leaveTypes);
        }
    }
}
