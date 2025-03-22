using System.ComponentModel.DataAnnotations;
using AspnetCoreMvcFull.Models;

namespace AspnetCoreMvcFull.DTOs
{
  public class LoginRequest
  {
    [Required(ErrorMessage = "Username is required")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
  }

  public class AuthResponse
  {
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public Employee? User { get; set; }
  }
}
