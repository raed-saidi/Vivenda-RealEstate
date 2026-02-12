using RealEstateMarketplace.BLL.DTOs;
using RealEstateMarketplace.DAL.Entities;
using RealEstateMarketplace.DAL.Repositories;

namespace RealEstateMarketplace.BLL.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public CategoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
    {
        var categories = await _unitOfWork.Repository<Category>().GetAllAsync();
        var result = new List<CategoryDto>();
        
        foreach (var category in categories)
        {
            var propertyCount = await _unitOfWork.Properties.CountAsync(p => p.CategoryId == category.Id);
            result.Add(new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IconClass = category.IconClass,
                IsActive = category.IsActive,
                PropertyCount = propertyCount
            });
        }
        
        return result;
    }
    
    public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
    {
        var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id);
        if (category == null) return null;
        
        var propertyCount = await _unitOfWork.Properties.CountAsync(p => p.CategoryId == category.Id);
        
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IconClass = category.IconClass,
            IsActive = category.IsActive,
            PropertyCount = propertyCount
        };
    }
    
    public async Task<CategoryDto> CreateCategoryAsync(CategoryDto dto)
    {
        var category = new Category
        {
            Name = dto.Name,
            Description = dto.Description,
            IconClass = dto.IconClass,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };
        
        await _unitOfWork.Repository<Category>().AddAsync(category);
        
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IconClass = category.IconClass,
            IsActive = category.IsActive
        };
    }
    
    public async Task<CategoryDto> UpdateCategoryAsync(CategoryDto dto)
    {
        var category = await _unitOfWork.Repository<Category>().GetByIdAsync(dto.Id);
        if (category == null)
            throw new Exception("Category not found");
            
        category.Name = dto.Name;
        category.Description = dto.Description;
        category.IconClass = dto.IconClass;
        category.IsActive = dto.IsActive;
        
        await _unitOfWork.Repository<Category>().UpdateAsync(category);
        
        return dto;
    }
    
    public async Task DeleteCategoryAsync(int id)
    {
        var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id);
        if (category != null)
        {
            await _unitOfWork.Repository<Category>().DeleteAsync(category);
        }
    }
}
