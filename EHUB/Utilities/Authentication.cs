using EHUB.Models.Administration;
using EHUB.Models.Login;
using EHUB.Utilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Newtonsoft.Json;
using System.Security.Claims;
namespace EHUB.Utilities
{
    public class AuthUser : ActionFilterAttribute
    {
        public async override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary {
                                { "Controller", "Login" },
                                { "Action", "Index" }
            });
            }
        }
    }
    public class AuthFull : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var uinfo = filterContext.HttpContext.User.Claims.ToArray();
            var page = filterContext.HttpContext.Request.Path.Value.Split('/')[2];
            GM gM = new GM();
            string chk = gM.pageaccess(page, uinfo[3].Value);
            if (chk != "f")
            {
                filterContext.Result = new RedirectToRouteResult(
                new RouteValueDictionary {
                                { "Controller", "Home" },
                                { "Action", "Index" }
                            });
            }
        }
    }
    public class AuthWrite : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var uinfo = filterContext.HttpContext.User.Claims.ToArray();
            var page = filterContext.HttpContext.Request.Path.Value.Split('/')[2];
            GM gM = new GM();
            string chk = gM.pageaccess(page, uinfo[3].Value);
            if ((chk == "r" || chk == "") && page != "StaffProfile")
            {
                filterContext.Result = new RedirectToRouteResult(
                new RouteValueDictionary {
                                { "Controller", "Home" },
                                { "Action", "Index" }
                            });
            }
        }
    }
    public class AuthRights : ActionFilterAttribute
    {
        private HttpContext _httpContext => new HttpContextAccessor().HttpContext;
        private string ch;
        GM gM = new GM();
        public string chk()
        {
            var uinfo = _httpContext.User.Claims.ToArray();
            var page = "StaffRecord";
            try
            {
                page = _httpContext.Request.Path.Value.Split('/')[2];
                if (_httpContext.Request.Path.Value.Split('/')[1] == "home")
                {
                    page = "StaffRecord";
                }
            }
            catch { }
            ch = gM.pageaccess(page, uinfo[3].Value);
            return ch;
        }
        public int mgr()
        {
            var uinfo = _httpContext.User.Claims.ToArray();
            return (int)gM.FillDSet($"SELECT COUNT(1) FROM tblEmployees WHERE (RManager = {uinfo[2].Value})").Tables[0].Rows[0][0];
        }
    }
}
