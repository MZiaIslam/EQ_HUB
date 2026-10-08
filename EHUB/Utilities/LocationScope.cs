using EHUB.Models.Administration;
using EHUB.Models.HRManagement;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EHUB.Utilities
{
	/// <summary>
	/// Resolves which work locations the signed-in user may see and which one is currently selected.
	/// Administrators (full rights on the Locations page) can access every active location;
	/// everyone else is limited to the locations assigned to them in staff registration.
	/// The selection is kept in a cookie and re-validated on every request.
	/// </summary>
	public class LocationScope
	{
		public const string CookieName = "ehub_loc";
		private const string AdminPage = "WorkLocations";
		private readonly DataContext _dContext;
		private readonly IHttpContextAccessor _http;
		private readonly GM _gm;
		private List<WorkLocation>? _allowed;
		private bool? _isAdmin;
		private bool? _configured;

		public LocationScope(DataContext dContext, IHttpContextAccessor http, GM gm)
		{
			_dContext = dContext;
			_http = http;
			_gm = gm;
		}

		private ClaimsPrincipal? User => _http.HttpContext?.User;

		public int EmpId => int.TryParse(User?.FindFirst(ClaimTypes.Sid)?.Value, out var id) ? id : 0;

		public bool IsAdmin => _isAdmin ??= User?.FindFirst(ClaimTypes.GroupSid)?.Value is string g
			&& _gm.pageaccess(AdminPage, g) == "f";

		/// <summary>Locations the user may work with.</summary>
		public List<WorkLocation> Allowed
		{
			get
			{
				if (_allowed != null) return _allowed;
				if (EmpId == 0) return _allowed = new List<WorkLocation>();
				var sql = "SELECT L.LocationId, L.LocationName, L.Address, L.City, L.Phone, L.IsActive FROM tblWorkLocations L WHERE L.IsDel = 0 AND L.IsActive = 1 "
					+ (IsAdmin ? "" : "AND L.LocationId IN (SELECT LocationId FROM tblEmpWorkLocations WHERE EmpID = @EmpID) ")
					+ "ORDER BY L.LocationName";
				try
				{
					_allowed = _dContext.Database.SqlQueryRaw<WorkLocation>(sql, new SqlParameter("@EmpID", EmpId)).ToList();
				}
				catch
				{
					// Tables not deployed yet (Database/Locations.sql).
					_allowed = new List<WorkLocation>();
				}
				return _allowed;
			}
		}

		/// <summary>False until at least one location exists (or if the tables are not deployed): nothing is restricted then.</summary>
		public bool Configured => _configured ??= HasLocations();

		private bool HasLocations()
		{
			try
			{
				return _dContext.Database.SqlQueryRaw<int>("SELECT COUNT(1) AS [Value] FROM tblWorkLocations WHERE IsDel = 0").ToList().FirstOrDefault() > 0;
			}
			catch
			{
				return false;
			}
		}

		/// <summary>True when the user may pick between locations or see a consolidated view.</summary>
		public bool CanSwitch => Allowed.Count > 1;

		/// <summary>Selected location id, or 0 for consolidated (all allowed locations).</summary>
		public int Active
		{
			get
			{
				if (Allowed.Count == 1) return Allowed[0].LocationId;
				if (int.TryParse(_http.HttpContext?.Request.Cookies[CookieName], out var id) && Allowed.Any(l => l.LocationId == id))
					return id;
				return 0;
			}
		}

		public string ActiveName => Active == 0
			? (Allowed.Count > 1 ? "All Locations" : "No Location")
			: Allowed.First(l => l.LocationId == Active).LocationName;

		/// <summary>Location ids the current view should cover: the selected one, or all allowed when consolidated.</summary>
		public List<int> ScopeIds => Active != 0 ? new List<int> { Active } : Allowed.Select(l => l.LocationId).ToList();

		/// <summary>Comma separated ids for passing to reports / stored procedures. Empty string means none.</summary>
		public string ScopeCsv => string.Join(",", ScopeIds);

		public bool TrySelect(int locationId)
		{
			var ctx = _http.HttpContext;
			if (ctx == null) return false;
			if (locationId != 0 && !Allowed.Any(l => l.LocationId == locationId)) return false;
			ctx.Response.Cookies.Append(CookieName, locationId.ToString(), new CookieOptions
			{
				HttpOnly = true,
				Secure = ctx.Request.IsHttps,
				SameSite = SameSiteMode.Lax,
				Expires = DateTimeOffset.UtcNow.AddYears(1)
			});
			return true;
		}

		/// <summary>Employee ids belonging to the current scope (plus the user, so they never lose their own record).</summary>
		public HashSet<int> ScopedEmpIds()
		{
			var ids = ScopeIds;
			var result = new HashSet<int> { EmpId };
			if (ids.Count == 0) return result;
			var rows = _dContext.Database.SqlQueryRaw<int>(
				"SELECT DISTINCT EmpID AS [Value] FROM tblEmpWorkLocations WHERE LocationId IN (" + string.Join(",", ids) + ")").ToList();
			result.UnionWith(rows);
			return result;
		}

		/// <summary>Restrict a staff list to the current location scope.</summary>
		public List<StaffData> FilterStaff(IEnumerable<StaffData> staff)
		{
			if (!Configured) return staff.ToList();
			var emps = ScopedEmpIds();
			return staff.Where(s => emps.Contains(s.empid)).ToList();
		}

		public List<SelectListItem> SelectItems() =>
			Allowed.Select(l => new SelectListItem(l.LocationName, l.LocationId.ToString(), l.LocationId == Active)).ToList();
	}
}
