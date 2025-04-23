using Microsoft.AspNetCore.Mvc;
using AspnetCoreMvcFull.Services;
using Microsoft.AspNetCore.Authorization;

namespace AspnetCoreMvcFull.Controllers
{
  [Authorize]
  public class DashboardsController : Controller
  {
    private readonly ICraneService _craneService;
    private readonly IDashboardService _dashboardService;

    public DashboardsController(ICraneService craneService, IDashboardService dashboardService)
    {
      _craneService = craneService;
      _dashboardService = dashboardService;
    }

    public async Task<IActionResult> Index()
    {
      try
      {
        // Get all cranes for dropdown
        var cranes = await _craneService.GetAllCranesAsync();
        ViewBag.Cranes = cranes;

        return View();
      }
      catch (Exception ex)
      {
        // Log error
        Console.Error.WriteLine($"Error in Dashboard Index: {ex.Message}");
        Console.Error.WriteLine(ex.StackTrace);

        // Return error view
        return View("Error");
      }
    }
  }
}
