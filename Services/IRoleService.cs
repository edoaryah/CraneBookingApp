using AspnetCoreMvcFull.DTOs;
using AspnetCoreMvcFull.Models;

namespace AspnetCoreMvcFull.Services
{
  public interface IRoleService
  {
    Task<IEnumerable<RoleDto>> GetAllRolesAsync();
    Task<RoleDto?> GetRoleByIdAsync(int id);
    Task<RoleDto?> GetRoleByNameAsync(string name);
    Task<RoleDto> CreateRoleAsync(RoleCreateDto roleDto, string createdBy);
    Task<RoleDto> UpdateRoleAsync(int id, RoleUpdateDto roleDto, string updatedBy);
    Task DeleteRoleAsync(int id);

    // User-role operations
    Task<IEnumerable<UserRoleDto>> GetUsersByRoleIdAsync(int roleId);
    Task<IEnumerable<UserRoleDto>> GetUsersByRoleNameAsync(string roleName);
    Task<UserRoleDto?> GetUserRoleByIdAsync(int id);
    Task<UserRoleDto?> GetUserRoleByLdapUserAndRoleIdAsync(string ldapUser, int roleId);
    Task<UserRoleDto> AssignRoleToUserAsync(UserRoleCreateDto userRoleDto, string createdBy);
    Task<UserRoleDto> UpdateUserRoleAsync(int id, UserRoleUpdateDto userRoleDto, string updatedBy);
    Task RemoveRoleFromUserAsync(int userRoleId);
    Task<bool> UserHasRoleAsync(string ldapUser, string roleName);
    Task<bool> UserHasRoleAsync(string ldapUser, int roleId);
    Task<IEnumerable<Employee>> GetEmployeesNotInRoleAsync(int roleId, string? department = null);
  }
}
