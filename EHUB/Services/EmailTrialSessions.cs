using EHUB.Models.Home;
using EHUB.Models.HRManagement;
using EHUB.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
namespace EHUB.Services
{
    public interface IEmailTrialSessions
    {
        Task CheckAndSendDailyRemindersAsync();
    }
    public class EmailTrialSessions : IEmailTrialSessions
    {
        private readonly DataContext _db;
        private readonly ILogger<EmailTrialSessions> _logger;
        GM gM = new GM();
        public EmailTrialSessions(DataContext db, ILogger<EmailTrialSessions> logger)
        {
            _db = db;
            _logger = logger;
        }
        public async Task CheckAndSendDailyRemindersAsync()
        {
            // 1. Fetch items/tasks due today from the database
            var pendingReminders = await _db.Database.SqlQueryRaw<ReminderDto>(@"usp_EmailTrialSessions").ToListAsync();
            // 2. Loop through and send emails
            foreach (var item in pendingReminders)
            {
                try
                {
                    // Send mail logic here (e.g., MailKit or API)
                    gM.SendEmail(item.Email, $"Reminder: Trial Session for {item.StudentName} - {item.StartTime:MMMM dd, yyyy}", $"Dear {item.Guardian},<br><br>This is a reminder for the trial session scheduled for {item.StudentName} on {item.StartTime:MMMM dd, yyyy} from {item.StartTime:hh:mm tt} to {item.EndTime:hh:mm tt}.<br>Please ensure that the student is prepared and available for the session.<br>Thank you for your attention.<br><br>Best regards,<br/><img src='https://equationhubportal.ca/dist/img/eqhub.png' alt='eqhub' style='width: 100px;'><br/><b>Equation Hub</b><br/>2 Orchard Heights Boulevard<br/>Aurora, Ontario L4G 6T5, Canada<br/>(905) 409-6284<br/>www.equationhub.ca", null);
                    var em = _db.Database.SqlQueryRaw<EmpIds>($"UPDATE tblSessionSchedule SET IsReminderSent = 1 WHERE  (Id = {item.Id})").ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send reminder for ID {Id}", item.Id);
                }
            }
            // 3. Save updated states back to DB
            //await _db.SaveChangesAsync();
        }
    }
    public class ReminderDto
    {
        public int Id { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string Guardian { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
