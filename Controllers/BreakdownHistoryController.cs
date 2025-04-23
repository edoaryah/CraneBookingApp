// Controllers/BreakdownHistoryController.cs
using Microsoft.AspNetCore.Mvc;
using AspnetCoreMvcFull.Filters;

namespace AspnetCoreMvcFull.Controllers
{
  [ServiceFilter(typeof(AuthorizationFilter))]
  public class BreakdownHistoryController : Controller
  {
    public IActionResult Index(int? craneId = null)
    {
      ViewData["CraneId"] = craneId;
      return View();
    }
  }
}
