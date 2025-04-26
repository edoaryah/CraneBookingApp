// Controllers/BookingFormController.cs
using Microsoft.AspNetCore.Mvc;
using AspnetCoreMvcFull.Filters;

namespace AspnetCoreMvcFull.Controllers
{
  [ServiceFilter(typeof(AuthorizationFilter))]
  public class BookingFormController : Controller
  {
    public IActionResult Index()
    {
      // Get user data from claims
      var userName = User.FindFirst("name")?.Value ?? "";
      var userDepartment = User.FindFirst("department")?.Value ?? "";
      var userLdap = User.FindFirst("ldap")?.Value ?? "";

      // Pass user data to view
      ViewData["UserName"] = userName;
      ViewData["UserDepartment"] = userDepartment;
      ViewData["UserLdap"] = userLdap;

      return View();
    }
  }
}
