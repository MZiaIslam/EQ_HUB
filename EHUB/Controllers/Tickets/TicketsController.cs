using EHUB.Utilities;
using EHUB.Models.Login;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.JavaScript;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using EHUB.Models.Leaves;
using EHUB.Models.Tickets;
using EHUB.Models.HRManagement;
using EHUB.Models.Administration;

namespace EHUB.Controllers.Tickets
{
    [AuthUser]
  
    public class TicketsController : Controller
    {
		private readonly DataContext _dContext;
		private readonly IWebHostEnvironment _wHostEnv;
		private readonly GM _gm;
		public TicketsController(DataContext dContext, IWebHostEnvironment wHostEnv, GM gm)
		{
			_dContext = dContext;
			_wHostEnv = wHostEnv;
			_gm = gm;
		}
        [AuthWrite]
        public ActionResult NewTicket()
		{
			var uinfo = User.Claims.ToArray();

			var lsttickets = _dContext.Database.SqlQuery<Ticket>($"SELECT tblTickets.*, tblLibraries.cls FROM tblTickets INNER JOIN tblLibraries ON tblTickets.stat = tblLibraries.LibName WHERE  (tblTickets.CreatedBy = {uinfo[2].Value})").ToList();
            var _dpart = _dContext.Database.SqlQuery<Departments>($"SELECT * FROM tblDepartments WHERE (HeadEmp IS NOT NULL) AND (ParentId IS NOT NULL)").ToList();
            var data = new List<object> { lsttickets, _dpart };

            return View(data);
		}
		[HttpGet]
		public async Task<ActionResult> getTicketInfoAsync(string id)
		{
			var  lsttickets = await _dContext.Database.SqlQuery<TicketsInfo>($"usp_getTicketInfo {id}").ToListAsync();
			return Json(lsttickets);
		}
		[HttpPost]
		public async Task<ActionResult> AddTicketAsync(TInfo dInfo)
		{


			if (ModelState.IsValid)
			{
				var uinfo = User.Claims.ToArray();
				var _docs = await _dContext.Database.SqlQueryRaw<EmpIds>
					($"usp_AddTicket @empid,@subject,@details,@depart",
				new SqlParameter("@subject", dInfo.subject),
                new SqlParameter("@depart", dInfo.depart),
                new SqlParameter("@empid", uinfo[2].Value),
				new SqlParameter("@details", dInfo.details)).ToListAsync();

			}

			return RedirectToAction("NewTicket", "Tickets");

		}
		[HttpPost]
		public async Task<ActionResult> AddMessageAsync(TInfo dInfo)
		{


			if (ModelState.IsValid)
			{
				var uinfo = User.Claims.ToArray();
				var _docs = await _dContext.Database.SqlQueryRaw<EmpIds>
					($"usp_AddTicketMSG @empid,@details,@ticketid",
				new SqlParameter("@ticketid", dInfo.ticketid),
				new SqlParameter("@empid", uinfo[2].Value),
				new SqlParameter("@details", dInfo.message)).ToListAsync();

			}
			var lsttickets = await _dContext.Database.SqlQuery<TicketsInfo>($"usp_getTicketInfo {dInfo.ticketid}").ToListAsync();
			return Json(lsttickets);
		

		}
        [AuthWrite]
        public ActionResult TicketsAssignments()
		{
			var uinfo = User.Claims.ToArray();
            var lsttickets = _dContext.Database.SqlQuery<Ticket>($"SELECT tblTickets.*, tblLibraries.cls FROM tblTickets INNER JOIN tblLibraries ON tblTickets.stat = tblLibraries.LibName INNER JOIN tblDepartments ON tblTickets.depart = tblDepartments.DepartID WHERE (tblDepartments.HeadEmp = {uinfo[2].Value})").ToList();
            if (uinfo[3].Value=="1")
			{
                lsttickets = _dContext.Database.SqlQuery<Ticket>($"SELECT tblTickets.*, tblLibraries.cls FROM tblTickets INNER JOIN tblLibraries ON tblTickets.stat = tblLibraries.LibName").ToList();
            }
			var emps = _dContext.Database.SqlQuery<StaffData>($"usp_StaffRecord").ToList();
            
            var lib = _dContext.Database.SqlQuery<Libs>($"SELECT * FROM tblLibraries where LiBType=2").ToList();
			var mdl = new TModel
			{
				Ticket = lsttickets,
			libs=lib,
				emps = emps
		
			};
			return View(mdl);
		}
        [HttpPost]
        public async Task<ActionResult> AssignTicketAsync(TInfo dInfo)
        {


            if (ModelState.IsValid)
            {
                var uinfo = User.Claims.ToArray();
                var _docs = await _dContext.Database.SqlQueryRaw<EmpIds>
                    ($"usp_AssignTicket @empid,@details,@ticketid,@assignto,@stat",
                
                new SqlParameter("@empid", uinfo[2].Value),
                 new SqlParameter("@details", (object)dInfo.message ?? ""),
                new SqlParameter("@ticketid", dInfo.ticketid),
                new SqlParameter("@assignto", dInfo.empid),
                new SqlParameter("@stat", dInfo.stat)).ToListAsync();

            }
            //var lsttickets = await _dContext.Database.SqlQuery<TicketsInfo>($"usp_getTicketInfo {dInfo.ticketid}").ToListAsync();
            //return Json(lsttickets);
            return RedirectToAction("TicketsAssignments", "Tickets");

        }
        [AuthWrite]
        public ActionResult AssignedTickets()
		{
			var uinfo = User.Claims.ToArray();

			var lsttickets = _dContext.Database.SqlQuery<Ticket>($"SELECT tblTickets.*, tblLibraries.cls FROM tblTickets INNER JOIN tblLibraries ON tblTickets.stat = tblLibraries.LibName WHERE  (tblTickets.assignto = {uinfo[2].Value})").ToList();

			return View(lsttickets);
		}
	}
}
