using EHUB.Models;
using EHUB.Models.Home;
using EHUB.Models.HRManagement;
using EHUB.Models.HRReports;
using EHUB.Models.Leaves;
using EHUB.Models.Login;
using EHUB.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System.Diagnostics;
using System.Security.Claims;
namespace EHUB.Controllers.HRReports
{
    [AuthUser]
    public class HRReportsController : Controller
    {
        private readonly ILogger<HRReportsController> _logger;
        private readonly DataContext _dContext;
        private static int rptid;
        GM gM = new GM();
        public HRReportsController(ILogger<HRReportsController> logger, DataContext dContext)
        {
            _logger = logger;
            _dContext = dContext;
        }
        public async Task<ActionResult> MyReports()
        {
            var uinfo = User.Claims.ToArray();
            var rpts = await _dContext.Database.SqlQueryRaw<Reports>(
                       "SELECT RptID, Title FROM tblReportHDR WHERE (UserID = @UserID)",
                       new SqlParameter("@UserID", uinfo[2].Value)
                   ).ToListAsync();
            return View(rpts);
        }
        [HttpGet]
        public IActionResult ReportView(int id)
        {
            rptid = id;
            return View();
        }
        public IActionResult ReportHeader()
        {
            string res = gM.FillDSet("SELECT Title+'|'+convert(varchar,Dashboard) FROM tblReportHDR where RptID = " + rptid).Tables[0].Rows[0][0].ToString();
            return Ok(res);
        }
        public IActionResult ReportDTL()
        {
            string res = gM.FillDSet("SELECT dbo.fn_RptDTLs('" + rptid + "')").Tables[0].Rows[0][0].ToString();
            return Ok(res);
        }
        public IActionResult DelSection(int ID)
        {
            gM.FillDSet($"DELETE FROM tblReportDTL WHERE (ID = {ID})");
            return Ok("");
        }
        public IActionResult updatetxt(string ID,string txt)
        {
            gM.FillDSet($"UPDATE tblReportDTL set remarks='{txt}'  WHERE (ID = {ID.Replace("t","")})");
            return Ok("");
        }
        [HttpGet]
        public IActionResult SetColSize(int ID, string size)
        {
            gM.FillDSet("UPDATE tblReportDTL SET width = '" + size + "' WHERE (ID = " + ID + ")");
            return Ok("");
        }
        public IActionResult SetDash(string id, string act)
        {
            var uinfo = User.Claims.ToArray();
            gM.FillDSet("UPDATE tblReportHDR SET Dashboard = 0 WHERE (UserID = " + uinfo[2].Value + ");UPDATE tblReportHDR SET Dashboard =" + act + " WHERE (UserID = " + uinfo[2].Value + ") AND (RptID = " + rptid + ")");
            return Ok("");
        }
        public IActionResult ReportSection(string info)
        {
            string[] dataID = info.Split(':');
            string res = gM.HTMLshTable(dataID[1] + " " + dataID[0], "chrt-tb" + dataID[2], "tb-css");
            return Ok(res);
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
