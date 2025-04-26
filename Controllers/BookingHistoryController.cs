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
    private readonly IRoleService _roleService;
    private readonly ILogger<BookingHistoryController> _logger;

    public BookingHistoryController(
        IBookingService bookingService,
        IRoleService roleService,
        ILogger<BookingHistoryController> logger)
    {
      _bookingService = bookingService;
      _roleService = roleService;
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
        // Get the current user's information from claims
        var ldapUser = User.FindFirst("ldapuser")?.Value;
        var userName = User.FindFirst("name")?.Value;

        if (string.IsNullOrEmpty(ldapUser))
        {
          _logger.LogWarning("User LDAP username not found in claims");
          return RedirectToAction("Login", "Auth");
        }

        // Get booking details
        var booking = await _bookingService.GetBookingByIdAsync(id);

        // Check if user has PIC role using the role service
        bool isPic = await _roleService.UserHasRoleAsync(ldapUser, "pic");

        // Check if current user is the booking creator
        bool isBookingCreator = (userName == booking.Name);

        // Pass role information to the view
        ViewData["IsPicRole"] = isPic;
        ViewData["IsBookingCreator"] = isBookingCreator;

        // Pass the booking to the view
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
        return View(null); // Send null model, view will display error message
      }
    }

    public IActionResult Approved()
    {
      ViewData["Title"] = "Approved Bookings";
      return View();
    }
  }
}
