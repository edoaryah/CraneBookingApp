using AspnetCoreMvcFull.DTOs;
using AspnetCoreMvcFull.Models;

namespace AspnetCoreMvcFull.Services
{
  public interface IUsageSubcategoryService
  {
    Task<IEnumerable<UsageSubcategoryDto>> GetAllSubcategoriesAsync();
    Task<IEnumerable<UsageSubcategoryDto>> GetSubcategoriesByCategoryAsync(UsageCategory category);
    Task<UsageSubcategoryDto> GetSubcategoryByIdAsync(int id);
    Task<UsageSubcategoryDto> CreateSubcategoryAsync(string name, UsageCategory category);
    Task<UsageSubcategoryDto> UpdateSubcategoryAsync(int id, string name, UsageCategory category);
    Task DeactivateSubcategoryAsync(int id);
    Task<bool> SubcategoryExistsAsync(int id);
  }
}
