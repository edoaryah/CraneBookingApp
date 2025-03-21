using Microsoft.AspNetCore.Mvc;

namespace AspnetCoreMvcFull.Controllers
{
  public class CalendarController : Controller
  {
    public IActionResult Index()
    {
      return View();
    }
  }
}
