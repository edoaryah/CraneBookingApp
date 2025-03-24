using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace AspnetCoreMvcFull.Middleware
{
  public class JwtCookieMiddleware
  {
    private readonly RequestDelegate _next;

    public JwtCookieMiddleware(RequestDelegate next)
    {
      _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
      // Jika ini adalah request API dan tidak ada Authorization header tetapi ada JWT cookie
      if (context.Request.Path.StartsWithSegments("/api") &&
          !context.Request.Headers.ContainsKey("Authorization") &&
          context.Request.Cookies.TryGetValue("jwt_token", out string? token) &&
          !string.IsNullOrEmpty(token))
      {
        // Tambahkan token sebagai Bearer token di Authorization header
        context.Request.Headers.Append("Authorization", $"Bearer {token}");
      }

      await _next(context);
    }
  }

  // Method extension untuk memudahkan penambahan middleware
  public static class JwtCookieMiddlewareExtensions
  {
    public static IApplicationBuilder UseJwtCookieMiddleware(this IApplicationBuilder builder)
    {
      return builder.UseMiddleware<JwtCookieMiddleware>();
    }
  }
}
