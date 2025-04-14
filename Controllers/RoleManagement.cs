using Microsoft.AspNetCore.Mvc;
using AspnetCoreMvcFull.Filters;

namespace AspnetCoreMvcFull.Controllers
{
  [ServiceFilter(typeof(AuthorizationFilter))]
  public class RoleManagementController : Controller
  {
    public IActionResult Index()
    {
      // Get user data from claims
      var userName = User.FindFirst("name")?.Value ?? "";
      var userDepartment = User.FindFirst("department")?.Value ?? "";
      var ldapUser = User.FindFirst("ldapuser")?.Value ?? "";

      // Pass user data to view
      ViewData["UserName"] = userName;
      ViewData["UserDepartment"] = userDepartment;
      ViewData["LdapUser"] = ldapUser;

      return View();
    }

    public IActionResult Users(int id)
    {
      // Pass role id to view
      ViewData["RoleId"] = id;

      return View();
    }
  }
}
