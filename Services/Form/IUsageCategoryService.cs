using AspnetCoreMvcFull.DTOs;
using AspnetCoreMvcFull.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AspnetCoreMvcFull.Services
{
  public interface IUsageCategoryService
  {
    Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
    string GetCategoryName(UsageCategory category);
  }
}
