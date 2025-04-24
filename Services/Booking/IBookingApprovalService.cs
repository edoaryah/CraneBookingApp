// Services/Booking/IBookingApprovalService.cs
using AspnetCoreMvcFull.Models;

namespace AspnetCoreMvcFull.Services
{
  public interface IBookingApprovalService
  {
    Task<bool> ApproveByManagerAsync(int bookingId, string managerName);
    Task<bool> RejectByManagerAsync(int bookingId, string managerName, string rejectReason);
    Task<bool> ApproveByPicAsync(int bookingId, string picName);
    Task<bool> RejectByPicAsync(int bookingId, string picName, string rejectReason);
    Task<bool> MarkAsDoneAsync(int bookingId, string picName);
  }
}
