using System.Security.Claims;
using AspnetCoreMvcFull.Models;

namespace AspnetCoreMvcFull.Services
{
  public interface IJwtService
  {
    string GenerateJwtToken(Employee employee);
    ClaimsPrincipal? ValidateToken(string token);
  }
}
