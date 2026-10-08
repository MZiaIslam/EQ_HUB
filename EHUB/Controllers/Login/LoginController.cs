using EHUB.Models.Login;
using EHUB.Utilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.JavaScript;
using System.Security.Claims;
using System.Security.Principal;
namespace EHUB.Controllers.Login
{
    public class LoginController : Controller
    {
        private readonly DataContext _dContext;
        GM gM = new GM();
        public LoginController(DataContext dContext)
        {
            _dContext = dContext;
        }
        public ActionResult Index()
        {
            gM.FillDSet("INSERT INTO tblLoginlog (loginid, stat, empid) VALUES ('0', 'Logout', 0)");
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> LoginAsync(LoginInfo login)
        {
            var epass = gM.HashPasword(login.pass);
            var uData = await _dContext.Database.SqlQueryRaw<LoginInfo>("usp_ULogin @email,@pass",
                new SqlParameter("@email", login.email),
                new SqlParameter("@pass", epass)).ToListAsync();
            if (uData.Count() > 0)
            {
                //var _unserInfo = JsonConvert.SerializeObject(uData, Formatting.Indented);
                //HttpContext.Session.SetString("UData", _unserInfo);
                var claims = new List<Claim>
                 {
                     new Claim(ClaimTypes.Name, uData[0].name),
                     new Claim(ClaimTypes.Email, uData[0].email),
                     new Claim(ClaimTypes.Sid, uData[0].empid.ToString()),
                     new Claim(ClaimTypes.GroupSid, uData[0].groupId.ToString()),
                     new Claim(ClaimTypes.GivenName, uData[0].designation),
                     new Claim(ClaimTypes.PostalCode, uData[0].depart),
                     new Claim(ClaimTypes.HomePhone, uData[0].hwork.ToString()),
			// Add additional claims as needed
		         };
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    // Set additional authentication properties if needed
                    ExpiresUtc = DateTimeOffset.UtcNow.AddYears(1) // match cookie
                };
                await HttpContext.SignInAsync(
                      CookieAuthenticationDefaults.AuthenticationScheme,
                      new ClaimsPrincipal(claimsIdentity),
                      authProperties);
                return RedirectToAction("index", "Home");
            }
            else
            {
                TempData["msg"] = "There was an error with your E-Mail/Password combination. Please try again.";
                return RedirectToAction("index", "Login");
            }
        }
        [HttpGet("logout")]
        public IActionResult Logout()
        {
            var uinfo = User.Claims.ToArray();
            gM.FillDSet("INSERT INTO tblLoginlog (loginid, stat, empid) VALUES ('" + uinfo[1].Value + "', 'Logout', " + uinfo[2].Value + ")");
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("index");
            //return Ok(new { success = true });
        }
        [HttpGet]
        public async Task<FileResult> AttendSheet(string mnth, int empid, int departid, string token)
        {
            var reportParams = $"usp_AttendSheet N'{mnth}',{empid},{departid}";
            byte[] bytes = await Task.Run(() => gM.RunReport("DataSet1".Split('¦'), reportParams.Split('¦'), "EHUB.AttdSheet.rdlc", "pdf"));
            const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            // Set response headers
            HttpContext.Response.ContentType = contentType;
            HttpContext.Response.Headers.Append("Access-Control-Expose-Headers", "Content-Disposition");
            // Return the file as a download
            var fileContentResult = new FileContentResult(bytes, contentType)
            {
                FileDownloadName = "AttendSheet.xlsx"
            };
            return fileContentResult;
        }
    }
}
