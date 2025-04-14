using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AspnetCoreMvcFull.DTOs;
using AspnetCoreMvcFull.Services;
using AspnetCoreMvcFull.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using AspnetCoreMvcFull.Models;

namespace AspnetCoreMvcFull.Controllers.Api
{
  [Route("api/[controller]")]
  [ApiController]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  [ServiceFilter(typeof(GlobalExceptionFilter))]
  public class RolesController : ControllerBase
  {
    private readonly IRoleService _roleService;
    private readonly ILogger<RolesController> _logger;

    public RolesController(IRoleService roleService, ILogger<RolesController> logger)
    {
      _roleService = roleService;
      _logger = logger;
    }

    #region Role Management

    // GET: api/Roles
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoleDto>>> GetRoles()
    {
      var roles = await _roleService.GetAllRolesAsync();
      return Ok(roles);
    }

    // GET: api/Roles/5
    [HttpGet("{id}")]
    public async Task<ActionResult<RoleDto>> GetRole(int id)
    {
      var role = await _roleService.GetRoleByIdAsync(id);

      if (role == null)
        return NotFound();

      return Ok(role);
    }

    // GET: api/Roles/ByName/admin
    [HttpGet("ByName/{name}")]
    public async Task<ActionResult<RoleDto>> GetRoleByName(string name)
    {
      var role = await _roleService.GetRoleByNameAsync(name);

      if (role == null)
        return NotFound();

      return Ok(role);
    }

    // POST: api/Roles
    [HttpPost]
    public async Task<ActionResult<RoleDto>> CreateRole(RoleCreateDto roleDto)
    {
      var currentUser = User.FindFirst("ldapuser")?.Value ?? "system";
      var createdRole = await _roleService.CreateRoleAsync(roleDto, currentUser);

      return CreatedAtAction(nameof(GetRole), new { id = createdRole.Id }, createdRole);
    }

    // PUT: api/Roles/5
    [HttpPut("{id}")]
    public async Task<ActionResult<RoleDto>> UpdateRole(int id, RoleUpdateDto roleDto)
    {
      var currentUser = User.FindFirst("ldapuser")?.Value ?? "system";

      try
      {
        var updatedRole = await _roleService.UpdateRoleAsync(id, roleDto, currentUser);
        return Ok(updatedRole);
      }
      catch (KeyNotFoundException)
      {
        return NotFound();
      }
      catch (InvalidOperationException ex)
      {
        return BadRequest(new { message = ex.Message });
      }
    }

    // DELETE: api/Roles/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRole(int id)
    {
      try
      {
        await _roleService.DeleteRoleAsync(id);
        return NoContent();
      }
      catch (KeyNotFoundException)
      {
        return NotFound();
      }
      catch (InvalidOperationException ex)
      {
        return BadRequest(new { message = ex.Message });
      }
    }

    #endregion

    #region User-Role Management

    // GET: api/Roles/5/Users
    [HttpGet("{roleId}/Users")]
    public async Task<ActionResult<IEnumerable<UserRoleDto>>> GetUsersByRoleId(int roleId)
    {
      var userRoles = await _roleService.GetUsersByRoleIdAsync(roleId);
      return Ok(userRoles);
    }

    // GET: api/Roles/ByName/admin/Users
    [HttpGet("ByName/{roleName}/Users")]
    public async Task<ActionResult<IEnumerable<UserRoleDto>>> GetUsersByRoleName(string roleName)
    {
      var userRoles = await _roleService.GetUsersByRoleNameAsync(roleName);
      return Ok(userRoles);
    }

    // GET: api/Roles/UserRole/5
    [HttpGet("UserRole/{id}")]
    public async Task<ActionResult<UserRoleDto>> GetUserRole(int id)
    {
      var userRole = await _roleService.GetUserRoleByIdAsync(id);

      if (userRole == null)
        return NotFound();

      return Ok(userRole);
    }

    // GET: api/Roles/CheckUserRole?ldapUser=PIC1&roleName=pic
    [HttpGet("CheckUserRole")]
    public async Task<ActionResult<bool>> CheckUserRoleByName([FromQuery] string ldapUser, [FromQuery] string roleName)
    {
      var hasRole = await _roleService.UserHasRoleAsync(ldapUser, roleName);
      return Ok(hasRole);
    }

    // GET: api/Roles/CheckUserRoleById?ldapUser=PIC1&roleId=1
    [HttpGet("CheckUserRoleById")]
    public async Task<ActionResult<bool>> CheckUserRoleById([FromQuery] string ldapUser, [FromQuery] int roleId)
    {
      var hasRole = await _roleService.UserHasRoleAsync(ldapUser, roleId);
      return Ok(hasRole);
    }

    // GET: api/Roles/5/AvailableEmployees?department=Department%20Name
    [HttpGet("{roleId}/AvailableEmployees")]
    public async Task<ActionResult<IEnumerable<Employee>>> GetAvailableEmployees(int roleId, [FromQuery] string? department = null)
    {
      var employees = await _roleService.GetEmployeesNotInRoleAsync(roleId, department);
      return Ok(employees);
    }

    // POST: api/Roles/AssignToUser
    [HttpPost("AssignToUser")]
    public async Task<ActionResult<UserRoleDto>> AssignRoleToUser(UserRoleCreateDto userRoleDto)
    {
      var currentUser = User.FindFirst("ldapuser")?.Value ?? "system";

      try
      {
        var userRole = await _roleService.AssignRoleToUserAsync(userRoleDto, currentUser);
        return CreatedAtAction(nameof(GetUserRole), new { id = userRole.Id }, userRole);
      }
      catch (KeyNotFoundException ex)
      {
        return NotFound(new { message = ex.Message });
      }
      catch (InvalidOperationException ex)
      {
        return BadRequest(new { message = ex.Message });
      }
    }

    // PUT: api/Roles/UserRole/5
    [HttpPut("UserRole/{id}")]
    public async Task<ActionResult<UserRoleDto>> UpdateUserRole(int id, UserRoleUpdateDto userRoleDto)
    {
      var currentUser = User.FindFirst("ldapuser")?.Value ?? "system";

      try
      {
        var userRole = await _roleService.UpdateUserRoleAsync(id, userRoleDto, currentUser);
        return Ok(userRole);
      }
      catch (KeyNotFoundException)
      {
        return NotFound();
      }
    }

    // DELETE: api/Roles/UserRole/5
    [HttpDelete("UserRole/{id}")]
    public async Task<IActionResult> RemoveRoleFromUser(int id)
    {
      try
      {
        await _roleService.RemoveRoleFromUserAsync(id);
        return NoContent();
      }
      catch (KeyNotFoundException)
      {
        return NotFound();
      }
    }

    #endregion
  }
}
