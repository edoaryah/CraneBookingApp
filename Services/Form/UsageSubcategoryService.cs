using Microsoft.EntityFrameworkCore;
using AspnetCoreMvcFull.Data;
using AspnetCoreMvcFull.DTOs;
using AspnetCoreMvcFull.Models;

namespace AspnetCoreMvcFull.Services
{
  public class UsageSubcategoryService : IUsageSubcategoryService
  {
    private readonly AppDbContext _context;
    private readonly ILogger<UsageSubcategoryService> _logger;

    public UsageSubcategoryService(
        AppDbContext context,
        ILogger<UsageSubcategoryService> logger)
    {
      _context = context;
      _logger = logger;
    }

    public async Task<IEnumerable<UsageSubcategoryDto>> GetAllSubcategoriesAsync()
    {
      var subcategories = await _context.UsageSubcategories
          .Where(s => s.IsActive)
          .OrderBy(s => s.Category)
          .ThenBy(s => s.Name)
          .ToListAsync();

      return subcategories.Select(MapToDto).ToList();
    }

    public async Task<IEnumerable<UsageSubcategoryDto>> GetSubcategoriesByCategoryAsync(UsageCategory category)
    {
      var subcategories = await _context.UsageSubcategories
          .Where(s => s.Category == category && s.IsActive)
          .OrderBy(s => s.Name)
          .ToListAsync();

      return subcategories.Select(MapToDto).ToList();
    }

    public async Task<UsageSubcategoryDto> GetSubcategoryByIdAsync(int id)
    {
      var subcategory = await _context.UsageSubcategories
          .FirstOrDefaultAsync(s => s.Id == id);

      if (subcategory == null)
      {
        throw new KeyNotFoundException($"Subcategory with ID {id} not found");
      }

      return MapToDto(subcategory);
    }

    public async Task<UsageSubcategoryDto> CreateSubcategoryAsync(string name, UsageCategory category)
    {
      try
      {
        _logger.LogInformation("Creating subcategory: {Name} in category {Category}", name, category);

        // Check if a subcategory with the same name already exists in this category
        var existingSubcategory = await _context.UsageSubcategories
            .FirstOrDefaultAsync(s => s.Name == name && s.Category == category);

        if (existingSubcategory != null)
        {
          // If it exists but is inactive, reactivate it
          if (!existingSubcategory.IsActive)
          {
            existingSubcategory.IsActive = true;
            await _context.SaveChangesAsync();
            return MapToDto(existingSubcategory);
          }

          throw new InvalidOperationException($"A subcategory with the name '{name}' already exists in category {category}");
        }

        var subcategory = new UsageSubcategory
        {
          Name = name,
          Category = category,
          IsActive = true
        };

        _context.UsageSubcategories.Add(subcategory);
        await _context.SaveChangesAsync();

        return MapToDto(subcategory);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error creating subcategory: {Message}", ex.Message);
        throw;
      }
    }

    public async Task<UsageSubcategoryDto> UpdateSubcategoryAsync(int id, string name, UsageCategory category)
    {
      try
      {
        _logger.LogInformation("Updating subcategory ID: {Id}", id);

        var subcategory = await _context.UsageSubcategories
            .FirstOrDefaultAsync(s => s.Id == id);

        if (subcategory == null)
        {
          throw new KeyNotFoundException($"Subcategory with ID {id} not found");
        }

        // Check if another subcategory with the same name already exists in this category
        var existingSubcategory = await _context.UsageSubcategories
            .FirstOrDefaultAsync(s => s.Id != id && s.Name == name && s.Category == category);

        if (existingSubcategory != null)
        {
          throw new InvalidOperationException($"Another subcategory with the name '{name}' already exists in category {category}");
        }

        subcategory.Name = name;
        subcategory.Category = category;

        await _context.SaveChangesAsync();

        return MapToDto(subcategory);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error updating subcategory: {Message}", ex.Message);
        throw;
      }
    }

    public async Task DeactivateSubcategoryAsync(int id)
    {
      try
      {
        _logger.LogInformation("Deactivating subcategory ID: {Id}", id);

        var subcategory = await _context.UsageSubcategories
            .FirstOrDefaultAsync(s => s.Id == id);

        if (subcategory == null)
        {
          throw new KeyNotFoundException($"Subcategory with ID {id} not found");
        }

        // Check if this subcategory is being used in any records
        var isInUse = await _context.CraneUsageRecords
            .AnyAsync(r => r.SubcategoryId == id);

        if (isInUse)
        {
          // Soft delete - just mark as inactive
          subcategory.IsActive = false;
        }
        else
        {
          // Hard delete if not in use
          _context.UsageSubcategories.Remove(subcategory);
        }

        await _context.SaveChangesAsync();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error deactivating subcategory: {Message}", ex.Message);
        throw;
      }
    }

    public async Task<bool> SubcategoryExistsAsync(int id)
    {
      return await _context.UsageSubcategories
          .AnyAsync(s => s.Id == id && s.IsActive);
    }

    private static UsageSubcategoryDto MapToDto(UsageSubcategory subcategory)
    {
      return new UsageSubcategoryDto
      {
        Id = subcategory.Id,
        Name = subcategory.Name,
        Category = subcategory.Category
      };
    }
  }
}
