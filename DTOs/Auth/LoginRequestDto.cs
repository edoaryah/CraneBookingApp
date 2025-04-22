using System.ComponentModel.DataAnnotations;

namespace AspnetCoreMvcFull.DTOs
{
  public class LoginRequestDto
  {
    [Required(ErrorMessage = "Username is required")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
  }
}
