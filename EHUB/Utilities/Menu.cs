using AspNetCore.ReportingServices.ReportProcessing.ReportObjectModel;
using EHUB.Models.Home;
using EHUB.Models.HRManagement;
using EHUB.Models.Leaves;
using EHUB.Models.Login;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;
namespace EHUB.Utilities
{
    [AuthUser]
    public class Menu
    {
        private readonly DataContext _dContext;
        private readonly IWebHostEnvironment _wHostEnv;
        private readonly GM _gm;
        //private readonly HttpContext _httpContext;
        GM gM = new GM();
        private HttpContext _httpContext => new HttpContextAccessor().HttpContext;
        public Menu(DataContext dContext, IWebHostEnvironment wHostEnv, GM gm)
        {
            _dContext = dContext;
            _wHostEnv = wHostEnv;
            _gm = gm;
        }
        public List<LeftMenu> GetMenu(string gId)
        {
            var menu = _dContext.Database.SqlQueryRaw<LeftMenu>("usp_MMenu @GroupID",
               new SqlParameter("@GroupID", gId)).ToList();
            return menu;
        }
        public List<LeftMenu> GetSMenu(string gId)
        {
            var menu = _dContext.Database.SqlQueryRaw<LeftMenu>("usp_SMenu @GroupID",
              new SqlParameter("@GroupID", gId)).ToList();
            return menu;
        }
        public GeneralRqst GetRequest(string gId)
        {
            var uinfo = _httpContext.User.Claims.ToArray();
            AuthRights authRights = new AuthRights();
            var chk = authRights.chk();
            var gRequests = _dContext.Database.SqlQueryRaw<GeneralRequests>("usp_HRGRequestsAprv @EmpId,@auth",
                new SqlParameter("@EmpID", uinfo[2].Value), new SqlParameter("@auth", chk)).ToList();
            var exptRequests = _dContext.Database.SqlQueryRaw<ExcptRequests>("usp_ExemptionRequestsAprovel @EmpId,@auth",
    new SqlParameter("@EmpID", uinfo[2].Value), new SqlParameter("@auth", chk)).ToList();
            var generalRqst = new GeneralRqst
            {
                generalrequest = gRequests.Where(x => x.aprvstatus == "To Approve").ToList(),
                excptRequests = exptRequests.Where(x => x.aprvstatus == "To Approve").ToList()
            };
            return generalRqst;
        }
    }
}
