using Microsoft.AspNetCore.Mvc;
using AspnetCoreMvcFull.Filters;

namespace AspnetCoreMvcFull.Controllers
{
  [ServiceFilter(typeof(AuthorizationFilter))]
  public class CalendarController : Controller
  {
    public IActionResult Index()
    {
      return View();
    }
  }
}
