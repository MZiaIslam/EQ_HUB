using EHUB.Controllers.Dashboard;
using EHUB.Models.Home;
using EHUB.Models.HRManagement;
using EHUB.Services;
using EHUB.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Data;
namespace EHUB.Controllers
{
    public class BaseController : Controller
    {
        private readonly DataContext _dContext;
        GM gM = new GM();
        public BaseController(DataContext dContext)
        {
            _dContext = dContext;
        }
        [AuthWrite]
        public override async Task OnActionExecutionAsync(ActionExecutingContext context,ActionExecutionDelegate next)
        {
            Console.WriteLine($"BaseController: {context.HttpContext.Request.Method} " + $"{context.HttpContext.Request.Path}");
            // User is not logged in
            if (User.Identity?.IsAuthenticated != true)
            {
                context.Result = new RedirectToActionResult(
                    "index",
                    "Login",
                    null);
                return;
            }
            var uinfo = User.Claims?.ToArray();
            var chkAdm = uinfo[3].Value != "18" && uinfo[3].Value != "19" ? true : false;
            if (chkAdm)
            {
                var notifications = await _dContext.Database.SqlQueryRaw<LowSessionNotification>("usp_LowSessionNotifications").ToListAsync();
                ViewBag.LowSessionNotifications = notifications;
            }
            await next();
        }
        public async Task CheckAndSendDailyRemindersAsync()
        {
            // 1. Fetch items/tasks due today from the database
            var pendingReminders = await _dContext.Database.SqlQueryRaw<ReminderDto>(@"usp_EmailTrialSessions").ToListAsync();
            // 2. Loop through and send emails
            foreach (var item in pendingReminders)
            {
                try
                {
                    // Send mail logic here (e.g., MailKit or API)
                    gM.SendEmail(item.Email, $"Reminder: Trial Session for {item.StudentName} - {item.StartTime:MMMM dd, yyyy}", $"Dear {item.Guardian},<br><br>This is a reminder for the trial session scheduled for {item.StudentName} on {item.StartTime:MMMM dd, yyyy} from {item.StartTime:hh:mm tt} to {item.EndTime:hh:mm tt}.<br>Please ensure that the student is prepared and available for the session.<br>Thank you for your attention.<br><br>Best regards,<br/><img src='https://equationhubportal.ca/dist/img/eqhub.png' alt='eqhub' style='width: 100px;'><br/><b>Equation Hub</b><br/>2 Orchard Heights Boulevard<br/>Aurora, Ontario L4G 6T5, Canada<br/>(905) 409-6284<br/>www.equationhub.ca", null);
                    var em = _dContext.Database.SqlQueryRaw<EmpIds>($"UPDATE tblSessionSchedule SET IsReminderSent = 1 WHERE  (Id = {item.Id})").ToList();
                }
                catch (Exception ex)
                {
                    
                }
            }

        }
    }
}
