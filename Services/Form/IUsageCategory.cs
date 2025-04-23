using AspnetCoreMvcFull.DTOs;
using AspnetCoreMvcFull.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspnetCoreMvcFull.Services
{
  public class UsageCategoryService : IUsageCategoryService
  {
    private readonly ILogger<UsageCategoryService> _logger;

    public UsageCategoryService(ILogger<UsageCategoryService> logger)
    {
      _logger = logger;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
    {
      try
      {
        _logger.LogInformation("Getting all usage categories");

        // Task.FromResult karena ini operasi sinkron tapi kita ingin API asinkron yang konsisten
        return await Task.FromResult(
            Enum.GetValues(typeof(UsageCategory))
                .Cast<UsageCategory>()
                .Select(c => new CategoryDto
                {
                  Id = (int)c,
                  Name = c.ToString()
                })
                .ToList()
        );
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error getting usage categories: {Message}", ex.Message);
        throw;
      }
    }

    public string GetCategoryName(UsageCategory category)
    {
      return category.ToString();
    }
  }
}
