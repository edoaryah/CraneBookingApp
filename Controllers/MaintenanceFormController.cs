using Microsoft.AspNetCore.Mvc;
using AspnetCoreMvcFull.Filters;

namespace AspnetCoreMvcFull.Controllers
{
  [ServiceFilter(typeof(AuthorizationFilter))]
  public class MaintenanceFormController : Controller
  {
    public IActionResult Index()
    {
      // Get user data from claims
      var userName = User.FindFirst("name")?.Value ?? "";
      var userDepartment = User.FindFirst("department")?.Value ?? "";

      // Pass user data to view
      ViewData["UserName"] = userName;
      ViewData["UserDepartment"] = userDepartment;

      return View();
    }
  }
}
