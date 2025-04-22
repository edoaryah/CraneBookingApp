using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using AspnetCoreMvcFull.Models;

namespace AspnetCoreMvcFull.Services
{
  public class JwtService : IJwtService
  {
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
      _configuration = configuration;
    }

    public string GenerateJwtToken(Employee employee)
    {
      var jwtKey = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("JWT Key is not configured in appsettings.json");

      var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
      // var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
      var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

      var claims = new[]
      {
                new Claim(JwtRegisteredClaimNames.Sub, employee.EmpId),
                new Claim(JwtRegisteredClaimNames.Name, employee.Name),
                new Claim("ldapuser", employee.LdapUser),
                new Claim("department", employee.Department),
                new Claim("division", employee.Division),
                new Claim("position", employee.PositionTitle),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

      var token = new JwtSecurityToken(
          issuer: _configuration["Jwt:Issuer"],
          audience: _configuration["Jwt:Audience"],
          claims: claims,
          expires: DateTime.Now.AddHours(8), // Token berlaku selama 8 jam
          signingCredentials: credentials
      );

      return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
      if (string.IsNullOrEmpty(token))
        return null;

      var tokenHandler = new JwtSecurityTokenHandler();
      // var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
      var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured");
      var key = Encoding.UTF8.GetBytes(jwtKey);

      try
      {
        var validationParameters = new TokenValidationParameters
        {
          ValidateIssuer = true,
          ValidateAudience = true,
          ValidateLifetime = true,
          ValidateIssuerSigningKey = true,
          ValidIssuer = _configuration["Jwt:Issuer"],
          ValidAudience = _configuration["Jwt:Audience"],
          IssuerSigningKey = new SymmetricSecurityKey(key)
        };

        SecurityToken validatedToken;
        return tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
      }
      catch
      {
        return null;
      }
    }
  }
}
