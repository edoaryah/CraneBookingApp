using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AspnetCoreMvcFull.Filters
{
  public class AuthorizationFilter : IAuthorizationFilter
  {
    private readonly ILogger<AuthorizationFilter> _logger;

    public AuthorizationFilter(ILogger<AuthorizationFilter> logger)
    {
      _logger = logger;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
      if (context.HttpContext.User.Identity?.IsAuthenticated != true)
      {
        _logger.LogWarning("Unauthorized access attempt to {Path}", context.HttpContext.Request.Path);

        // Redirect to login page with return URL
        var returnUrl = context.HttpContext.Request.Path;
        if (!string.IsNullOrEmpty(context.HttpContext.Request.QueryString.Value))
        {
          returnUrl += context.HttpContext.Request.QueryString.Value;
        }

        context.Result = new RedirectToActionResult("Login", "Auth", new { returnUrl });
      }
    }
  }
}
