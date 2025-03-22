using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using AspnetCoreMvcFull.DTOs;
using AspnetCoreMvcFull.Services;
using System.IdentityModel.Tokens.Jwt;

namespace AspnetCoreMvcFull.Controllers
{
  public class AuthController : Controller
  {
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
      _authService = authService;
      _logger = logger;
    }

    [HttpGet]
    public IActionResult Login(string returnUrl = "/")
    {
      // Jika pengguna sudah login, redirect ke halaman utama
      if (User.Identity?.IsAuthenticated == true)
      {
        return Redirect(returnUrl);
      }

      ViewData["ReturnUrl"] = returnUrl;
      return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginRequest model, string returnUrl = "/")
    {
      ViewData["ReturnUrl"] = returnUrl;

      if (!ModelState.IsValid)
      {
        return View(model);
      }

      try
      {
        var response = await _authService.LoginAsync(model);

        if (!response.Success)
        {
          ModelState.AddModelError(string.Empty, response.Message);
          return View(model);
        }

        // Parse the token to get claims
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(response.Token);

        // Create claims identity
        var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, response.User.EmpId),
                    new Claim(ClaimTypes.Name, response.User.Name),
                    new Claim("ldapuser", response.User.LdapUser),
                    new Claim("department", response.User.Department),
                    new Claim("division", response.User.Division),
                    new Claim("position", response.User.PositionTitle)
                };

        var claimsIdentity = new ClaimsIdentity(
            claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties
        {
          IsPersistent = model.RememberMe,
          ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
        };

        // Sign in user
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        // Store JWT token in a secure, httponly cookie
        Response.Cookies.Append("jwt_token", response.Token, new CookieOptions
        {
          HttpOnly = true,
          Secure = true, // Set to true in production with HTTPS
          SameSite = SameSiteMode.Strict,
          Expires = DateTimeOffset.UtcNow.AddHours(8)
        });

        _logger.LogInformation("User {Username} logged in successfully", model.Username);

        // Redirect to return URL or home page
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
          return Redirect(returnUrl);
        }
        else
        {
          return RedirectToAction("Index", "Calendar");
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error during login for user {Username}", model.Username);
        ModelState.AddModelError(string.Empty, "An error occurred during login. Please try again.");
        return View(model);
      }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
      // Sign out user
      await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

      // Remove JWT token cookie
      Response.Cookies.Delete("jwt_token");

      return RedirectToAction("Login", "Auth");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
      return View();
    }
  }
}
