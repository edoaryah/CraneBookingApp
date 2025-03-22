using Microsoft.AspNetCore.Mvc;
using AspnetCoreMvcFull.Filters;

namespace AspnetCoreMvcFull.Controllers
{
  [ServiceFilter(typeof(AuthorizationFilter))]
  public class BookingFormController : Controller
  {
    public IActionResult Index()
    {
      return View();
    }
  }
}
