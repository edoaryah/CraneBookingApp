using Microsoft.AspNetCore.Mvc;
using AspnetCoreMvcFull.Filters;
using AspnetCoreMvcFull.Services;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;

namespace AspnetCoreMvcFull.Controllers
{
  [ServiceFilter(typeof(AuthorizationFilter))]
  public class BookingHistoryController : Controller
  {
    private readonly IBookingService _bookingService;
    private readonly ILogger<BookingHistoryController> _logger;

    public BookingHistoryController(
        IBookingService bookingService,
        ILogger<BookingHistoryController> logger)
    {
      _bookingService = bookingService;
      _logger = logger;
    }

    public IActionResult Index()
    {
      return View();
    }

    public async Task<IActionResult> Details(int id)
    {
      try
      {
        // Mendapatkan detail booking dari service
        var booking = await _bookingService.GetBookingByIdAsync(id);

        // Meneruskan data booking ke view sebagai model
        return View(booking);
      }
      catch (KeyNotFoundException ex)
      {
        _logger.LogWarning(ex, "Booking dengan ID {id} tidak ditemukan", id);
        return NotFound();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Terjadi kesalahan saat memuat detail booking dengan ID {id}", id);
        return View(null); // Kirim model null, view akan menampilkan pesan error
      }
    }

    public IActionResult Approved()
    {
      ViewData["Title"] = "Approved Bookings";
      return View();
    }
  }
}
