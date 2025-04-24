// Services/Booking/BookingApprovalService.cs
using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.Data;
using Microsoft.EntityFrameworkCore;

namespace AspnetCoreMvcFull.Services
{
  public class BookingApprovalService : IBookingApprovalService
  {
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IEmployeeService _employeeService;
    private readonly ILogger<BookingApprovalService> _logger;

    public BookingApprovalService(
        AppDbContext context,
        IEmailService emailService,
        IEmployeeService employeeService,
        ILogger<BookingApprovalService> logger)
    {
      _context = context;
      _emailService = emailService;
      _employeeService = employeeService;
      _logger = logger;
    }

    public async Task<bool> ApproveByManagerAsync(int bookingId, string managerName)
    {
      try
      {
        var booking = await _context.Bookings
            .Include(b => b.Crane)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
        {
          _logger.LogWarning("Booking dengan ID {BookingId} tidak ditemukan", bookingId);
          return false;
        }

        // Memastikan booking dalam status yang benar
        if (booking.Status != BookingStatus.Pending)
        {
          _logger.LogWarning("Booking dengan ID {BookingId} tidak dalam status Pending", bookingId);
          return false;
        }

        // Update status booking menjadi ManagerApproved
        booking.Status = BookingStatus.ManagerApproved;
        booking.ManagerName = managerName;
        booking.ManagerApprovalTime = DateTime.Now;

        await _context.SaveChangesAsync();

        // Kirim notifikasi email ke user
        var user = await _employeeService.GetEmployeeByLdapUserAsync(booking.Name);
        if (user != null && !string.IsNullOrEmpty(user.Email))
        {
          await _emailService.SendBookingManagerApprovedEmailAsync(booking, user.Email);
        }

        // Kirim notifikasi email ke semua PIC crane
        var picCranes = await _employeeService.GetPicCraneAsync();
        foreach (var pic in picCranes)
        {
          if (!string.IsNullOrEmpty(pic.Email) && !string.IsNullOrEmpty(pic.LdapUser))
          {
            await _emailService.SendPicApprovalRequestEmailAsync(
                booking,
                pic.Email,
                pic.Name,
                pic.LdapUser);
          }
        }

        return true;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error saat menyetujui booking dengan ID {BookingId} oleh manager", bookingId);
        throw;
      }
    }

    public async Task<bool> RejectByManagerAsync(int bookingId, string managerName, string rejectReason)
    {
      try
      {
        var booking = await _context.Bookings
            .Include(b => b.Crane)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
        {
          _logger.LogWarning("Booking dengan ID {BookingId} tidak ditemukan", bookingId);
          return false;
        }

        // Memastikan booking dalam status yang benar
        if (booking.Status != BookingStatus.Pending)
        {
          _logger.LogWarning("Booking dengan ID {BookingId} tidak dalam status Pending", bookingId);
          return false;
        }

        // Update status booking menjadi ManagerRejected
        booking.Status = BookingStatus.ManagerRejected;
        booking.ManagerName = managerName;
        booking.ManagerApprovalTime = DateTime.Now;
        booking.ManagerRejectReason = rejectReason;

        await _context.SaveChangesAsync();

        // Kirim notifikasi email ke user
        var user = await _employeeService.GetEmployeeByLdapUserAsync(booking.Name);
        if (user != null && !string.IsNullOrEmpty(user.Email))
        {
          await _emailService.SendBookingRejectedEmailAsync(
              booking,
              user.Email,
              managerName,
              rejectReason);
        }

        return true;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error saat menolak booking dengan ID {BookingId} oleh manager", bookingId);
        throw;
      }
    }

    public async Task<bool> ApproveByPicAsync(int bookingId, string picName)
    {
      try
      {
        var booking = await _context.Bookings
            .Include(b => b.Crane)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
        {
          _logger.LogWarning("Booking dengan ID {BookingId} tidak ditemukan", bookingId);
          return false;
        }

        // Memastikan booking dalam status yang benar
        if (booking.Status != BookingStatus.ManagerApproved)
        {
          _logger.LogWarning("Booking dengan ID {BookingId} tidak dalam status ManagerApproved", bookingId);
          return false;
        }

        // Update status booking menjadi Approved
        booking.Status = BookingStatus.Approved;
        booking.PicName = picName;
        booking.PicApprovalTime = DateTime.Now;

        await _context.SaveChangesAsync();

        // Kirim notifikasi email ke user
        var user = await _employeeService.GetEmployeeByLdapUserAsync(booking.Name);
        if (user != null && !string.IsNullOrEmpty(user.Email))
        {
          await _emailService.SendBookingApprovedEmailAsync(booking, user.Email);
        }

        return true;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error saat menyetujui booking dengan ID {BookingId} oleh PIC", bookingId);
        throw;
      }
    }

    public async Task<bool> RejectByPicAsync(int bookingId, string picName, string rejectReason)
    {
      try
      {
        var booking = await _context.Bookings
            .Include(b => b.Crane)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
        {
          _logger.LogWarning("Booking dengan ID {BookingId} tidak ditemukan", bookingId);
          return false;
        }

        // Memastikan booking dalam status yang benar
        if (booking.Status != BookingStatus.ManagerApproved)
        {
          _logger.LogWarning("Booking dengan ID {BookingId} tidak dalam status ManagerApproved", bookingId);
          return false;
        }

        // Update status booking menjadi Rejected
        booking.Status = BookingStatus.Rejected;
        booking.PicName = picName;
        booking.PicApprovalTime = DateTime.Now;
        booking.PicRejectReason = rejectReason;

        await _context.SaveChangesAsync();

        // Kirim notifikasi email ke user
        var user = await _employeeService.GetEmployeeByLdapUserAsync(booking.Name);
        if (user != null && !string.IsNullOrEmpty(user.Email))
        {
          await _emailService.SendBookingRejectedEmailAsync(
              booking,
              user.Email,
              picName,
              rejectReason);
        }

        return true;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error saat menolak booking dengan ID {BookingId} oleh PIC", bookingId);
        throw;
      }
    }

    public async Task<bool> MarkAsDoneAsync(int bookingId, string picName)
    {
      try
      {
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
        {
          _logger.LogWarning("Booking dengan ID {BookingId} tidak ditemukan", bookingId);
          return false;
        }

        // Memastikan booking dalam status yang benar
        if (booking.Status != BookingStatus.Approved)
        {
          _logger.LogWarning("Booking dengan ID {BookingId} tidak dalam status Approved", bookingId);
          return false;
        }

        // Update status booking menjadi Done
        booking.Status = BookingStatus.Done;

        await _context.SaveChangesAsync();

        // Tidak perlu email notifikasi untuk pembaruan status menjadi Done

        return true;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error saat menandai booking dengan ID {BookingId} sebagai selesai", bookingId);
        throw;
      }
    }
  }
}
