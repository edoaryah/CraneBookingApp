using Microsoft.OpenApi.Models;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using AspnetCoreMvcFull.Data;
using AspnetCoreMvcFull.Services;
using AspnetCoreMvcFull.Filters;
using AspnetCoreMvcFull.Converters;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Register DbContext dengan connection string
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// Registrasi Service
builder.Services.AddScoped<ICraneService, CraneService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IHazardService, HazardService>();

// Registrasi Filter
builder.Services.AddScoped<GlobalExceptionFilter>();

// Menambahkan layanan untuk Controller dan Views
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
      // Tambahkan JsonStringEnumConverter ke daftar converter
      options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

      options.JsonSerializerOptions.Converters.Add(new DateTimeConverter());

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

app.UseAuthorization();

// Menyiapkan routing untuk controller
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Calendar}/{action=Index}/{id?}");

app.Run();
