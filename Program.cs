using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using System.Text;
using AspnetCoreMvcFull.Data;
using AspnetCoreMvcFull.Services;
using AspnetCoreMvcFull.Filters;
using AspnetCoreMvcFull.Events;
using AspnetCoreMvcFull.Events.Handlers;
using AspnetCoreMvcFull.Middleware;
using System.Text.Json.Serialization;
using AspnetCoreMvcFull.Converters;
using Microsoft.OpenApi.Any;

var builder = WebApplication.CreateBuilder(args);

// Tambahkan ini di bagian awal Program.cs, sebelum kode konfigurasi apapun
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Register DbContext dengan connection string
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// Registrasi Filter
builder.Services.AddScoped<GlobalExceptionFilter>();
builder.Services.AddScoped<AuthorizationFilter>();

// Tambahkan setelah registrasi service yang ada
// Registrasi event infrastructure
builder.Services.AddSingleton<IEventPublisher, EventPublisher>();
builder.Services.AddScoped<IEventHandler<CraneMaintenanceEvent>, BookingRelocationHandler>();

// Pastikan BookingService didaftarkan sebelum CraneService
// untuk menghindari masalah circular dependency
builder.Services.AddScoped<IHazardService, HazardService>();
builder.Services.AddScoped<IShiftDefinitionService, ShiftDefinitionService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IScheduleConflictService, ScheduleConflictService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IMaintenanceScheduleService, MaintenanceScheduleService>();
builder.Services.AddScoped<ICraneService, CraneService>();
// Tambahkan layanan baru
builder.Services.AddScoped<IUsageSubcategoryService, UsageSubcategoryService>();
builder.Services.AddScoped<ICraneUsageService, CraneUsageService>();

// Konfigurasi Autentikasi
builder.Services.AddAuthentication(options =>
{
  options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
  options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
  options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
  var jwtKey = builder.Configuration["Jwt:Key"];
  var key = Encoding.UTF8.GetBytes(jwtKey ?? throw new InvalidOperationException("JWT Key tidak ditemukan"));

  options.TokenValidationParameters = new TokenValidationParameters
  {
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    ValidIssuer = builder.Configuration["Jwt:Issuer"],
    ValidAudience = builder.Configuration["Jwt:Audience"],
    IssuerSigningKey = new SymmetricSecurityKey(key)
  };

  // Tambahkan events untuk menangani responses
  options.Events = new JwtBearerEvents
  {
    OnMessageReceived = context =>
    {
      // Cek cookie untuk token jika tidak ada di header
      if (string.IsNullOrEmpty(context.Token) &&
              context.Request.Cookies.TryGetValue("jwt_token", out string? token))
      {
        context.Token = token;
      }
      return Task.CompletedTask;
    },
    OnChallenge = context =>
    {
      // Override challenge response untuk API endpoints
      if (context.Request.Path.StartsWithSegments("/api"))
      {
        // Mencegah default challenge
        context.HandleResponse();

        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";

        return context.Response.WriteAsync("{\"message\":\"Anda perlu login untuk mengakses resource ini.\"}");
      }

      return Task.CompletedTask;
    }
  };
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
  options.Cookie.Name = "CraneBookingAuth";
  options.Cookie.HttpOnly = true;
  options.ExpireTimeSpan = TimeSpan.FromHours(8);
  options.SlidingExpiration = true;
  options.LoginPath = "/Auth/Login";
  options.LogoutPath = "/Auth/Logout";
  options.AccessDeniedPath = "/Auth/AccessDenied";
  options.Cookie.SameSite = SameSiteMode.Strict;
  options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

  // Tambahkan event handler untuk menangani MVC requests secara berbeda dari API
  options.Events = new CookieAuthenticationEvents
  {
    OnRedirectToLogin = context =>
    {
      // Jika request untuk endpoint API, kembalikan 401 daripada redirect
      if (context.Request.Path.StartsWithSegments("/api") ||
              (context.Request.Headers.TryGetValue("Accept", out var acceptHeaders) &&
               acceptHeaders.Any(h => h?.Contains("application/json") == true)))
      {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";
        return context.Response.WriteAsync("{\"message\":\"Anda perlu login untuk mengakses resource ini.\"}");
      }

      // Untuk request MVC biasa, redirect ke login
      context.Response.Redirect(context.RedirectUri);
      return Task.CompletedTask;
    },
    OnRedirectToAccessDenied = context =>
    {
      // Jika request untuk endpoint API, kembalikan 403 daripada redirect
      if (context.Request.Path.StartsWithSegments("/api") ||
              (context.Request.Headers.TryGetValue("Accept", out var acceptHeaders) &&
               acceptHeaders.Any(h => h?.Contains("application/json") == true)))
      {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.ContentType = "application/json";
        return context.Response.WriteAsync("{\"message\":\"Anda tidak memiliki izin untuk mengakses resource ini.\"}");
      }

      // Untuk request MVC biasa, redirect ke halaman access denied
      context.Response.Redirect(context.RedirectUri);
      return Task.CompletedTask;
    }
  };
});

// Menambahkan layanan untuk Controller dan Views
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
      // Tambahkan JsonStringEnumConverter ke daftar converter
      options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

      options.JsonSerializerOptions.Converters.Add(new DateTimeSecondsPrecisionConverter());

      // Opsional: Konfigurasi tambahan untuk JSON
      options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// swagger
builder.Services.AddSwaggerGen(c =>
{
  c.SwaggerDoc("v1", new OpenApiInfo
  {
    Title = "Crane API",
    Version = "v1",
    Description = "API CraneBookingApp"
  });

  // enum to string
  c.UseInlineDefinitionsForEnums();

  // Konfigurasi DateTime untuk menggunakan format lokal
  c.MapType<DateTime>(() => new OpenApiSchema
  {
    Type = "string",
    Format = "date-time",
    Example = new OpenApiString(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"))
  });

  // Konfigurasi Swagger untuk autentikasi
  c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
  {
    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
    Name = "Authorization",
    In = ParameterLocation.Header,
    Type = SecuritySchemeType.ApiKey,
    Scheme = "Bearer"
  });

  c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

// hangfire
builder.Services.AddHangfire(config =>
    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
          .UseSimpleAssemblyNameTypeSerializer()
          .UsePostgreSqlStorage(options =>
              options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"))
          ));

builder.Services.AddHangfireServer();

var app = builder.Build();

// Menyiapkan middleware
if (!app.Environment.IsDevelopment())
{
  app.UseExceptionHandler("/Home/Error");
  app.UseHsts();
}
else
{
  app.UseExceptionHandler("/Home/Error");
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
  c.SwaggerEndpoint("/swagger/v1/swagger.json", "Crane API v1");
  c.RoutePrefix = "swagger"; // Swagger UI ditampilkan di root URL
});

app.UseHangfireDashboard(); // Dashboard untuk memonitor background jobs dengan Hangfire

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Tambahkan middleware JWT Cookie SEBELUM middleware autentikasi
app.UseJwtCookieMiddleware();

// Tambahkan middleware autentikasi dan autorisasi
app.UseAuthentication();
app.UseAuthorization();

// Menyiapkan routing untuk controller
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Calendar}/{action=Index}/{id?}");

app.Run();
