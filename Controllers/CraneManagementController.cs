using Microsoft.AspNetCore.Mvc;
using AspnetCoreMvcFull.Filters;

namespace AspnetCoreMvcFull.Controllers
{
  [ServiceFilter(typeof(AuthorizationFilter))]
  public class CraneManagementController : Controller
  {
    public IActionResult Index()
    {
      return View();
    }
  }
}
