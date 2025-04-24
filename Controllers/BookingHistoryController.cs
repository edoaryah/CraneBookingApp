// [Controllers/BookingHistoryController.cs]
// Penambahan action Approved untuk PIC.
using Microsoft.AspNetCore.Mvc;
using AspnetCoreMvcFull.Filters;

namespace AspnetCoreMvcFull.Controllers
{
  [ServiceFilter(typeof(AuthorizationFilter))]
  public class BookingHistoryController : Controller
  {
    public IActionResult Index()
    {
      return View();
    }

    public IActionResult Details(int id)
    {
      ViewData["BookingId"] = id;
      return View();
    }

    // Controllers/BookingHistoryController.cs - tambahkan action untuk melihat daftar booking yang disetujui
    public async Task<IActionResult> Approved()
    {
      // Halaman ini akan menampilkan daftar booking yang sudah disetujui
      // dan memungkinkan PIC untuk menandainya sebagai selesai
      ViewData["Title"] = "Approved Bookings";
      return View();
    }
  }
}
