using Microsoft.AspNetCore.Mvc;
using AspnetCoreMvcFull.Filters;

namespace AspnetCoreMvcFull.Controllers
{
  [ServiceFilter(typeof(AuthorizationFilter))]
  public class MaintenanceHistoryController : Controller
  {
    public IActionResult Index()
    {
      return View();
    }

    public IActionResult Details(int id)
    {
      ViewData["MaintenanceId"] = id;
      return View();
    }
  }
}
