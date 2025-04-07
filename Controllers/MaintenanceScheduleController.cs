// 8. Buat controller MVC untuk halaman MaintenanceSchedule

// Controllers/MaintenanceScheduleController.cs
using Microsoft.AspNetCore.Mvc;
using AspnetCoreMvcFull.Filters;

namespace AspnetCoreMvcFull.Controllers
{
  [ServiceFilter(typeof(AuthorizationFilter))]
  public class MaintenanceScheduleController : Controller
  {
    public IActionResult Index()
    {
      return View();
    }

    public IActionResult Create()
    {
      return View();
    }

    public IActionResult Edit(int id)
    {
      ViewData["MaintenanceId"] = id;
      return View();
    }

    public IActionResult Details(int id)
    {
      ViewData["MaintenanceId"] = id;
      return View();
    }
  }
}
