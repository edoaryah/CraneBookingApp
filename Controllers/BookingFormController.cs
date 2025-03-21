using Microsoft.AspNetCore.Mvc;

namespace AspnetCoreMvcFull.Controllers
{
  public class BookingFormController : Controller
  {
    public IActionResult Index()
    {
      return View();
    }
  }
}
