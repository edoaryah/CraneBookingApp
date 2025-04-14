using AspnetCoreMvcFull.Data;
using AspnetCoreMvcFull.DTOs;
using AspnetCoreMvcFull.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AspnetCoreMvcFull.Services
{
  public class RoleService : IRoleService
  {
    private readonly AppDbContext _context;
    private readonly ILogger<RoleService> _logger;
    private readonly string _sqlServerConnectionString;
    private readonly IAuthService _authService;

    public RoleService(
        AppDbContext dbContext,
        ILogger<RoleService> logger,
        IConfiguration configuration,
        IAuthService authService)
    {
      _context = dbContext;
      _logger = logger;
      _sqlServerConnectionString = configuration.GetConnectionString("SqlServerConnection") ??
          throw new InvalidOperationException("SqlServerConnection is not configured");
      _authService = authService;
    }

    #region Role Management

    public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
    {
      try
      {
        var roles = await _context.Roles
            .Select(r => new RoleDto
            {
              Id = r.Id,
              Name = r.Name,
              Description = r.Description,
              IsActive = r.IsActive,
              CreatedAt = r.CreatedAt,
              CreatedBy = r.CreatedBy,
              UpdatedAt = r.UpdatedAt,
              UpdatedBy = r.UpdatedBy,
              UserCount = r.UserRoles.Count
            })
            .ToListAsync();

        return roles;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error getting all roles");
        throw;
      }
    }

    public async Task<RoleDto?> GetRoleByIdAsync(int id)
    {
      try
      {
        var role = await _context.Roles
            .Where(r => r.Id == id)
            .Select(r => new RoleDto
            {
              Id = r.Id,
              Name = r.Name,
              Description = r.Description,
              IsActive = r.IsActive,
              CreatedAt = r.CreatedAt,
              CreatedBy = r.CreatedBy,
              UpdatedAt = r.UpdatedAt,
              UpdatedBy = r.UpdatedBy,
              UserCount = r.UserRoles.Count
            })
            .FirstOrDefaultAsync();

        return role;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error getting role by id: {Id}", id);
        throw;
      }
    }

    public async Task<RoleDto?> GetRoleByNameAsync(string name)
    {
      try
      {
        var role = await _context.Roles
            .Where(r => r.Name.ToLower() == name.ToLower())
            .Select(r => new RoleDto
            {
              Id = r.Id,
              Name = r.Name,
              Description = r.Description,
              IsActive = r.IsActive,
              CreatedAt = r.CreatedAt,
              CreatedBy = r.CreatedBy,
              UpdatedAt = r.UpdatedAt,
              UpdatedBy = r.UpdatedBy,
              UserCount = r.UserRoles.Count
            })
            .FirstOrDefaultAsync();

        return role;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error getting role by name: {Name}", name);
        throw;
      }
    }

    public async Task<RoleDto> CreateRoleAsync(RoleCreateDto roleDto, string createdBy)
    {
      try
      {
        // Check if role with the same name already exists
        var existingRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.Name.ToLower() == roleDto.Name.ToLower());

        if (existingRole != null)
        {
          throw new InvalidOperationException($"Role with name '{roleDto.Name}' already exists");
        }

        var role = new Role
        {
          Name = roleDto.Name,
          Description = roleDto.Description,
          IsActive = roleDto.IsActive,
          CreatedAt = DateTime.Now,
          CreatedBy = createdBy
        };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        return new RoleDto
        {
          Id = role.Id,
          Name = role.Name,
          Description = role.Description,
          IsActive = role.IsActive,
          CreatedAt = role.CreatedAt,
          CreatedBy = role.CreatedBy,
          UpdatedAt = role.UpdatedAt,
          UpdatedBy = role.UpdatedBy,
          UserCount = 0 // New role has no users
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error creating role: {Name}", roleDto.Name);
        throw;
      }
    }

    public async Task<RoleDto> UpdateRoleAsync(int id, RoleUpdateDto roleDto, string updatedBy)
    {
      try
      {
        var role = await _context.Roles
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (role == null)
        {
          throw new KeyNotFoundException($"Role with ID {id} not found");
        }

        // Check if name is unique if it's being updated
        if (!string.IsNullOrEmpty(roleDto.Name) && roleDto.Name != role.Name)
        {
          var existingRole = await _context.Roles
              .FirstOrDefaultAsync(r => r.Name.ToLower() == roleDto.Name.ToLower());

          if (existingRole != null)
          {
            throw new InvalidOperationException($"Role with name '{roleDto.Name}' already exists");
          }

          role.Name = roleDto.Name;
        }

        if (roleDto.Description != null)
        {
          role.Description = roleDto.Description;
        }

        if (roleDto.IsActive.HasValue)
        {
          role.IsActive = roleDto.IsActive.Value;
        }

        role.UpdatedAt = DateTime.Now;
        role.UpdatedBy = updatedBy;

        _context.Roles.Update(role);
        await _context.SaveChangesAsync();

        return new RoleDto
        {
          Id = role.Id,
          Name = role.Name,
          Description = role.Description,
          IsActive = role.IsActive,
          CreatedAt = role.CreatedAt,
          CreatedBy = role.CreatedBy,
          UpdatedAt = role.UpdatedAt,
          UpdatedBy = role.UpdatedBy,
          UserCount = role.UserRoles.Count
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error updating role with ID: {Id}", id);
        throw;
      }
    }

    public async Task DeleteRoleAsync(int id)
    {
      try
      {
        var role = await _context.Roles
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (role == null)
        {
          throw new KeyNotFoundException($"Role with ID {id} not found");
        }

        // Check if the role has assigned users
        if (role.UserRoles.Any())
        {
          throw new InvalidOperationException($"Cannot delete role '{role.Name}' because it is assigned to {role.UserRoles.Count} users. Remove all user assignments first.");
        }

        _context.Roles.Remove(role);
        await _context.SaveChangesAsync();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error deleting role with ID: {Id}", id);
        throw;
      }
    }

    #endregion

    #region User-Role Management

    public async Task<IEnumerable<UserRoleDto>> GetUsersByRoleIdAsync(int roleId)
    {
      try
      {
        var userRoles = await _context.UserRoles
            .Include(ur => ur.Role)
            .Where(ur => ur.RoleId == roleId)
            .ToListAsync();

        var userRoleDtos = new List<UserRoleDto>();

        foreach (var userRole in userRoles)
        {
          var employee = await _authService.GetEmployeeByLdapUserAsync(userRole.LdapUser);

          userRoleDtos.Add(new UserRoleDto
          {
            Id = userRole.Id,
            LdapUser = userRole.LdapUser,
            RoleId = userRole.RoleId,
            RoleName = userRole.Role?.Name ?? string.Empty,
            Notes = userRole.Notes,
            CreatedAt = userRole.CreatedAt,
            CreatedBy = userRole.CreatedBy,
            UpdatedAt = userRole.UpdatedAt,
            UpdatedBy = userRole.UpdatedBy,
            EmployeeName = employee?.Name,
            EmployeeId = employee?.EmpId,
            Department = employee?.Department,
            Position = employee?.PositionTitle
          });
        }

        return userRoleDtos;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error getting users by role ID: {RoleId}", roleId);
        throw;
      }
    }

    public async Task<IEnumerable<UserRoleDto>> GetUsersByRoleNameAsync(string roleName)
    {
      try
      {
        var userRoles = await _context.UserRoles
            .Include(ur => ur.Role)
            .Where(ur => ur.Role != null && ur.Role.Name.ToLower() == roleName.ToLower())
            .ToListAsync();

        var userRoleDtos = new List<UserRoleDto>();

        foreach (var userRole in userRoles)
        {
          var employee = await _authService.GetEmployeeByLdapUserAsync(userRole.LdapUser);

          userRoleDtos.Add(new UserRoleDto
          {
            Id = userRole.Id,
            LdapUser = userRole.LdapUser,
            RoleId = userRole.RoleId,
            RoleName = userRole.Role?.Name ?? string.Empty,
            Notes = userRole.Notes,
            CreatedAt = userRole.CreatedAt,
            CreatedBy = userRole.CreatedBy,
            UpdatedAt = userRole.UpdatedAt,
            UpdatedBy = userRole.UpdatedBy,
            EmployeeName = employee?.Name,
            EmployeeId = employee?.EmpId,
            Department = employee?.Department,
            Position = employee?.PositionTitle
          });
        }

        return userRoleDtos;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error getting users by role name: {RoleName}", roleName);
        throw;
      }
    }

    public async Task<UserRoleDto?> GetUserRoleByIdAsync(int id)
    {
      try
      {
        var userRole = await _context.UserRoles
            .Include(ur => ur.Role)
            .FirstOrDefaultAsync(ur => ur.Id == id);

        if (userRole == null)
        {
          return null;
        }

        var employee = await _authService.GetEmployeeByLdapUserAsync(userRole.LdapUser);

        return new UserRoleDto
        {
          Id = userRole.Id,
          LdapUser = userRole.LdapUser,
          RoleId = userRole.RoleId,
          RoleName = userRole.Role?.Name ?? string.Empty,
          Notes = userRole.Notes,
          CreatedAt = userRole.CreatedAt,
          CreatedBy = userRole.CreatedBy,
          UpdatedAt = userRole.UpdatedAt,
          UpdatedBy = userRole.UpdatedBy,
          EmployeeName = employee?.Name,
          EmployeeId = employee?.EmpId,
          Department = employee?.Department,
          Position = employee?.PositionTitle
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error getting user role by ID: {Id}", id);
        throw;
      }
    }

    public async Task<UserRoleDto?> GetUserRoleByLdapUserAndRoleIdAsync(string ldapUser, int roleId)
    {
      try
      {
        var userRole = await _context.UserRoles
            .Include(ur => ur.Role)
            .FirstOrDefaultAsync(ur => ur.LdapUser == ldapUser && ur.RoleId == roleId);

        if (userRole == null)
        {
          return null;
        }

        var employee = await _authService.GetEmployeeByLdapUserAsync(userRole.LdapUser);

        return new UserRoleDto
        {
          Id = userRole.Id,
          LdapUser = userRole.LdapUser,
          RoleId = userRole.RoleId,
          RoleName = userRole.Role?.Name ?? string.Empty,
          Notes = userRole.Notes,
          CreatedAt = userRole.CreatedAt,
          CreatedBy = userRole.CreatedBy,
          UpdatedAt = userRole.UpdatedAt,
          UpdatedBy = userRole.UpdatedBy,
          EmployeeName = employee?.Name,
          EmployeeId = employee?.EmpId,
          Department = employee?.Department,
          Position = employee?.PositionTitle
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error getting user role by LDAP user and role ID: {LdapUser}, {RoleId}", ldapUser, roleId);
        throw;
      }
    }

    public async Task<UserRoleDto> AssignRoleToUserAsync(UserRoleCreateDto userRoleDto, string createdBy)
    {
      try
      {
        // Verify the role exists
        var role = await _context.Roles.FindAsync(userRoleDto.RoleId);
        if (role == null)
        {
          throw new KeyNotFoundException($"Role with ID {userRoleDto.RoleId} not found");
        }

        // Check if the user already has this role
        var existingUserRole = await _context.UserRoles
            .FirstOrDefaultAsync(ur => ur.LdapUser == userRoleDto.LdapUser && ur.RoleId == userRoleDto.RoleId);

        if (existingUserRole != null)
        {
          throw new InvalidOperationException($"User {userRoleDto.LdapUser} already has the role {role.Name}");
        }

        // Verify the user exists in employee database
        var employee = await _authService.GetEmployeeByLdapUserAsync(userRoleDto.LdapUser);
        if (employee == null)
        {
          throw new KeyNotFoundException($"Employee with LDAP user {userRoleDto.LdapUser} not found");
        }

        var userRole = new UserRole
        {
          LdapUser = userRoleDto.LdapUser,
          RoleId = userRoleDto.RoleId,
          Notes = userRoleDto.Notes,
          CreatedAt = DateTime.Now,
          CreatedBy = createdBy
        };

        _context.UserRoles.Add(userRole);
        await _context.SaveChangesAsync();

        return new UserRoleDto
        {
          Id = userRole.Id,
          LdapUser = userRole.LdapUser,
          RoleId = userRole.RoleId,
          RoleName = role.Name,
          Notes = userRole.Notes,
          CreatedAt = userRole.CreatedAt,
          CreatedBy = userRole.CreatedBy,
          UpdatedAt = userRole.UpdatedAt,
          UpdatedBy = userRole.UpdatedBy,
          EmployeeName = employee.Name,
          EmployeeId = employee.EmpId,
          Department = employee.Department,
          Position = employee.PositionTitle
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error assigning role to user: {LdapUser}, {RoleId}", userRoleDto.LdapUser, userRoleDto.RoleId);
        throw;
      }
    }

    public async Task<UserRoleDto> UpdateUserRoleAsync(int id, UserRoleUpdateDto userRoleDto, string updatedBy)
    {
      try
      {
        var userRole = await _context.UserRoles
            .Include(ur => ur.Role)
            .FirstOrDefaultAsync(ur => ur.Id == id);

        if (userRole == null)
        {
          throw new KeyNotFoundException($"User role with ID {id} not found");
        }

        userRole.Notes = userRoleDto.Notes;
        userRole.UpdatedAt = DateTime.Now;
        userRole.UpdatedBy = updatedBy;

        _context.UserRoles.Update(userRole);
        await _context.SaveChangesAsync();

        var employee = await _authService.GetEmployeeByLdapUserAsync(userRole.LdapUser);

        return new UserRoleDto
        {
          Id = userRole.Id,
          LdapUser = userRole.LdapUser,
          RoleId = userRole.RoleId,
          RoleName = userRole.Role?.Name ?? string.Empty,
          Notes = userRole.Notes,
          CreatedAt = userRole.CreatedAt,
          CreatedBy = userRole.CreatedBy,
          UpdatedAt = userRole.UpdatedAt,
          UpdatedBy = userRole.UpdatedBy,
          EmployeeName = employee?.Name,
          EmployeeId = employee?.EmpId,
          Department = employee?.Department,
          Position = employee?.PositionTitle
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error updating user role with ID: {Id}", id);
        throw;
      }
    }

    public async Task RemoveRoleFromUserAsync(int userRoleId)
    {
      try
      {
        var userRole = await _context.UserRoles.FindAsync(userRoleId);
        if (userRole == null)
        {
          throw new KeyNotFoundException($"User role with ID {userRoleId} not found");
        }

        _context.UserRoles.Remove(userRole);
        await _context.SaveChangesAsync();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error removing role from user with ID: {Id}", userRoleId);
        throw;
      }
    }

    public async Task<bool> UserHasRoleAsync(string ldapUser, string roleName)
    {
      try
      {
        return await _context.UserRoles
            .Include(ur => ur.Role)
            .AnyAsync(ur => ur.LdapUser == ldapUser &&
                            ur.Role != null &&
                            ur.Role.Name.ToLower() == roleName.ToLower() &&
                            ur.Role.IsActive);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error checking if user has role: {LdapUser}, {RoleName}", ldapUser, roleName);
        throw;
      }
    }

    public async Task<bool> UserHasRoleAsync(string ldapUser, int roleId)
    {
      try
      {
        return await _context.UserRoles
            .Include(ur => ur.Role)
            .AnyAsync(ur => ur.LdapUser == ldapUser &&
                            ur.RoleId == roleId &&
                            ur.Role != null &&
                            ur.Role.IsActive);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error checking if user has role: {LdapUser}, {RoleId}", ldapUser, roleId);
        throw;
      }
    }

    public async Task<IEnumerable<Employee>> GetEmployeesNotInRoleAsync(int roleId, string? department = null)
    {
      try
      {
        // Get all LDAP users who already have the specified role
        var existingRoleUsers = await _context.UserRoles
            .Where(r => r.RoleId == roleId)
            .Select(r => r.LdapUser)
            .ToListAsync();

        // If department is specified, get employees from that department
        // If not, get all employees
        List<Employee> employees = new List<Employee>();

        using (SqlConnection connection = new SqlConnection(_sqlServerConnectionString))
        {
          await connection.OpenAsync();

          string query;
          SqlCommand command;

          if (!string.IsNullOrEmpty(department))
          {
            query = "SELECT * FROM SP_EMPLIST WHERE DEPARTMENT = @Department";
            command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Department", department);
          }
          else
          {
            query = "SELECT * FROM SP_EMPLIST";
            command = new SqlCommand(query, connection);
          }

          using (SqlDataReader reader = await command.ExecuteReaderAsync())
          {
            while (await reader.ReadAsync())
            {
              employees.Add(new Employee
              {
                EmpId = reader["EMP_ID"]?.ToString() ?? string.Empty,
                Name = reader["NAME"]?.ToString() ?? string.Empty,
                PositionTitle = reader["POSITION_TITLE"]?.ToString() ?? string.Empty,
                Division = reader["DIVISION"]?.ToString() ?? string.Empty,
                Department = reader["DEPARTMENT"]?.ToString() ?? string.Empty,
                Email = reader["EMAIL"]?.ToString() ?? string.Empty,
                PositionLvl = reader["POSITION_LVL"]?.ToString() ?? string.Empty,
                LdapUser = reader["LDAPUSER"]?.ToString() ?? string.Empty,
                EmpStatus = reader["EMP_STATUS"]?.ToString() ?? string.Empty
              });
            }
          }
        }

        // Filter employees who are not already in the role
        return employees.Where(e => !string.IsNullOrEmpty(e.LdapUser) && !existingRoleUsers.Contains(e.LdapUser));
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error getting employees not in role: {RoleId}, {Department}", roleId, department);
        throw;
      }
    }

    #endregion
  }
}
