using Microsoft.Data.SqlClient;
using AspnetCoreMvcFull.DTOs;
using AspnetCoreMvcFull.Models;
using System.Data;

namespace AspnetCoreMvcFull.Services
{
  public class AuthService : IAuthService
  {
    private readonly string _connectionString;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IConfiguration configuration, IJwtService jwtService, ILogger<AuthService> logger)
    {
      _connectionString = configuration.GetConnectionString("SqlServerConnection")
                ?? throw new InvalidOperationException("SQL Server connection string 'SqlServerConnection' not found in configuration");
      // _connectionString = configuration.GetConnectionString("SqlServerConnection");
      _jwtService = jwtService;
      _logger = logger;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
      if (request == null)
      {
        throw new ArgumentNullException(nameof(request));
      }

      // Di lingkungan development, kita hanya memeriksa apakah username dan password sama
      // Ini akan diganti dengan API perusahaan ketika deployment di lingkungan produksi
      if (request.Username != request.Password)
      {
        _logger.LogWarning("Login failed: Invalid credentials for user {Username}", request.Username);
        return new AuthResponse
        {
          Success = false,
          Message = "Invalid username or password"
        };
      }

      // Cari user di database berdasarkan LDAPUSER
      var employee = await GetEmployeeByLdapUserAsync(request.Username);
      if (employee == null)
      {
        _logger.LogWarning("Login failed: User {Username} not found in database", request.Username);
        return new AuthResponse
        {
          Success = false,
          Message = "User not found"
        };
      }

      // Generate JWT token
      var token = _jwtService.GenerateJwtToken(employee);

      _logger.LogInformation("User {Username} logged in successfully", request.Username);
      return new AuthResponse
      {
        Success = true,
        Token = token,
        User = employee,
        Message = "Login successful"
      };
    }

    public async Task<Employee?> GetEmployeeByLdapUserAsync(string ldapUser)
    {
      if (string.IsNullOrEmpty(ldapUser))
      {
        throw new ArgumentException("LDAP username cannot be null or empty", nameof(ldapUser));
      }

      Employee? employee = null;

      try
      {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
          await connection.OpenAsync();

          string query = "SELECT * FROM SP_EMPLIST WHERE LDAPUSER = @LdapUser";
          using (SqlCommand command = new SqlCommand(query, connection))
          {
            command.Parameters.AddWithValue("@LdapUser", ldapUser);

            using (SqlDataReader reader = await command.ExecuteReaderAsync())
            {
              if (await reader.ReadAsync())
              {
                employee = new Employee
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
                };
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error fetching employee data for LDAP user {LdapUser}", ldapUser);
        throw;
      }

      return employee;
    }
  }
}
