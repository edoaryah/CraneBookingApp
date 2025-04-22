using AspnetCoreMvcFull.DTOs;
using AspnetCoreMvcFull.Models;

namespace AspnetCoreMvcFull.Services
{
  public interface IAuthService
  {
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    Task<Employee?> GetEmployeeByLdapUserAsync(string ldapUser);
  }
}
