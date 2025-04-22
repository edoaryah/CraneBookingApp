using Microsoft.AspNetCore.Mvc;
using AspnetCoreMvcFull.DTOs;
using AspnetCoreMvcFull.Services;
using AspnetCoreMvcFull.Filters;
using System.Threading.Tasks;

namespace AspnetCoreMvcFull.Controllers.Api
{
  [Route("api/auth")]
  [ApiController]
  [ServiceFilter(typeof(GlobalExceptionFilter))]
  public class AuthApiController : ControllerBase
  {
    private readonly IAuthService _authService;
    private readonly ILogger<AuthApiController> _logger;

    public AuthApiController(IAuthService authService, ILogger<AuthApiController> logger)
    {
      _authService = authService;
      _logger = logger;
    }

    /// <summary>
    /// Endpoint untuk mendapatkan token JWT untuk akses API
    /// </summary>
    /// <param name="model">Login credentials</param>
    /// <returns>Token JWT jika autentikasi berhasil</returns>
    [HttpPost("login")]
    public async Task<ActionResult<AuthTokenResponse>> Login([FromBody] LoginRequestDto model)
    {
      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      try
      {
        var response = await _authService.LoginAsync(model);

        if (!response.Success || response.User == null)
        {
          return Unauthorized(new { Message = response.Message });
        }

        return Ok(new AuthTokenResponse
        {
          Success = true,
          Message = "Login successful",
          Token = response.Token,
          User = new UserInfoDto
          {
            EmpId = response.User.EmpId,
            Name = response.User.Name,
            Department = response.User.Department,
            Division = response.User.Division,
            PositionTitle = response.User.PositionTitle,
            PositionLvl = response.User.PositionLvl,
            Email = response.User.Email,
            LdapUser = response.User.LdapUser,
            EmpStatus = response.User.EmpStatus
          }
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error during API login for user {Username}", model.Username);
        return StatusCode(500, new { Message = "An error occurred during login. Please try again." });
      }
    }
  }
}
