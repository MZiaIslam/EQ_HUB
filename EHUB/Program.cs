using EHUB.Services;
using EHUB.Utilities;
//using Hangfire;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddDbContext<DataContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("ConnectionStr")));
builder.Services.AddControllers();
// Add services to the container.
builder.Services.AddControllersWithViews();
//=====================AddHangfire======================
//builder.Services.AddHangfire(config => config
//    .UseSqlServerStorage(builder.Configuration.GetConnectionString("ConnectionStr")));
//builder.Services.AddHangfireServer();
//// Register your email service
//builder.Services.AddTransient<IEmailTrialSessions, EmailTrialSessions>();
//======================================================
//builder.Services.AddSession();
//builder.Services.AddSession(options =>
//{
//    options.IdleTimeout = TimeSpan.FromMinutes(160); // <-- your session timeout
//    options.Cookie.HttpOnly = true;
//    options.Cookie.IsEssential = true;
//});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<Menu>();
builder.Services.AddScoped<GM>();
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + @"Keys")) // persist keys so cookies survive app restarts
    .SetApplicationName("EHUB");
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "auth";
        options.ExpireTimeSpan = TimeSpan.FromDays(365); // access duration for particular cookie
        options.SlidingExpiration = true;
        // Make login persistent across browser restarts
        options.Events.OnSigningIn = async ctx =>
        {
            ctx.Properties.IsPersistent = true;
            ctx.Properties.ExpiresUtc = DateTimeOffset.UtcNow.AddYears(1); // override per user
            await Task.CompletedTask;
        };
    });
var app = builder.Build();
//app.UseSession();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();  // Enable authentication
app.UseAuthorization();
//app.UseSession();
//=====================AddHangfire======================
//app.UseHangfireDashboard();
//RecurringJob.AddOrUpdate<IEmailTrialSessions>(
//    "daily-reminder-job",
//    email => email.CheckAndSendDailyRemindersAsync(),
//    Cron.Daily(3, 0),
//    new RecurringJobOptions
//    {
//        TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")
//    }
//);
//===========================================================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
