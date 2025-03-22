using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;

namespace AspnetCoreMvcFull.Filters
{
  public class GlobalExceptionFilter : IExceptionFilter
  {
    private readonly ILogger<GlobalExceptionFilter> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger, IHostEnvironment env)
    {
      _logger = logger;
      _env = env;
    }

    public void OnException(ExceptionContext context)
    {
      _logger.LogError(context.Exception, context.Exception.Message);

      var response = new ApiErrorResponse
      {
        TraceId = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier ?? Guid.NewGuid().ToString(),
        Success = false,
        Message = _env.IsDevelopment()
              ? context.Exception.Message
              : "An unexpected error occurred"
      };

      // Customize status code based on exception type
      switch (context.Exception)
      {
        case DbUpdateConcurrencyException:
          context.HttpContext.Response.StatusCode = StatusCodes.Status409Conflict;
          response.Message = "Data was modified by another user. Please refresh and try again.";
          break;
        case DbUpdateException:
          context.HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
          response.Message = "Database error occurred while processing your request.";
          break;
        case KeyNotFoundException:
          context.HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
          response.Message = "The requested resource was not found.";
          break;
        case ArgumentException:
        case InvalidOperationException:
          context.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
          break;
        default:
          context.HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
          break;
      }

      // Add exception details in development environment
      if (_env.IsDevelopment())
      {
        response.DeveloperMessage = context.Exception.StackTrace;
      }

      context.Result = new JsonResult(response);
      context.ExceptionHandled = true;
    }
  }

  public class ApiErrorResponse
  {
    public string TraceId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? DeveloperMessage { get; set; }

    public ApiErrorResponse()
    {
    }

    public ApiErrorResponse(string message)
    {
      Message = message;
    }
  }
}
