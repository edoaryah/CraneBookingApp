// Services/Employee/IEmployeeService.cs
using AspnetCoreMvcFull.Models;

namespace AspnetCoreMvcFull.Services
{
  public interface IEmployeeService
  {
    Task<IEnumerable<Employee>> GetAllEmployeesAsync();
    Task<Employee?> GetEmployeeByLdapUserAsync(string ldapUser);
    Task<Employee?> GetManagerByDepartmentAsync(string department);
    Task<IEnumerable<Employee>> GetPicCraneAsync();
  }
}
