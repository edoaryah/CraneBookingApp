using AspnetCoreMvcFull.DTOs;
using AspnetCoreMvcFull.Models;

namespace AspnetCoreMvcFull.Services
{
  public interface IAuthService
  {
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<Employee?> GetEmployeeByLdapUserAsync(string ldapUser);
  }
}
