// // Controllers/CraneUsageController.cs
// using Microsoft.AspNetCore.Mvc;
// using AspnetCoreMvcFull.Filters;
// using AspnetCoreMvcFull.Services;

// namespace AspnetCoreMvcFull.Controllers
// {
//   [ServiceFilter(typeof(AuthorizationFilter))]
//   public class CraneUsageController : Controller
//   {
//     private readonly IBookingService _bookingService;

//     public CraneUsageController(IBookingService bookingService)
//     {
//       _bookingService = bookingService;
//     }

//     public IActionResult Index()
//     {
//       return View();
//     }

//     public async Task<IActionResult> LogUsage(int id)
//     {
//       try
//       {
//         var booking = await _bookingService.GetBookingByIdAsync(id);
//         ViewData["BookingId"] = id;
//         ViewData["BookingNumber"] = booking.BookingNumber;
//         ViewData["CraneCode"] = booking.CraneCode;
//         ViewData["StartDate"] = booking.StartDate.ToString("yyyy-MM-dd");
//         ViewData["EndDate"] = booking.EndDate.ToString("yyyy-MM-dd");
//         return View();
//       }
//       catch (KeyNotFoundException)
//       {
//         return NotFound();
//       }
//     }

//     public async Task<IActionResult> Summary(int id)
//     {
//       try
//       {
//         var booking = await _bookingService.GetBookingByIdAsync(id);
//         ViewData["BookingId"] = id;
//         ViewData["BookingNumber"] = booking.BookingNumber;
//         ViewData["CraneCode"] = booking.CraneCode;
//         return View();
//       }
//       catch (KeyNotFoundException)
//       {
//         return NotFound();
//       }
//     }
//   }
// }

using Microsoft.AspNetCore.Mvc;
using AspnetCoreMvcFull.Filters;

namespace AspnetCoreMvcFull.Controllers
{
  [ServiceFilter(typeof(AuthorizationFilter))]
  public class CraneUsageController : Controller
  {
    public IActionResult Index(int id)
    {
      ViewData["BookingId"] = id;
      return View();
    }

    public IActionResult History()
    {
      return View();
    }
  }
}
