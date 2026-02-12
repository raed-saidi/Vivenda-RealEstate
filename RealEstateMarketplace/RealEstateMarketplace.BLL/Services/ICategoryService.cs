using RealEstateMarketplace.BLL.DTOs;

namespace RealEstateMarketplace.BLL.Services;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
    Task<CategoryDto?> GetCategoryByIdAsync(int id);
    Task<CategoryDto> CreateCategoryAsync(CategoryDto dto);
    Task<CategoryDto> UpdateCategoryAsync(CategoryDto dto);
    Task DeleteCategoryAsync(int id);
}
