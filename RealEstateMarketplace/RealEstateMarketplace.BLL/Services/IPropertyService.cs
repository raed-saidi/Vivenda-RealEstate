using RealEstateMarketplace.BLL.DTOs;

namespace RealEstateMarketplace.BLL.Services;

public interface IPropertyService
{
    Task<IEnumerable<PropertyDto>> GetAllPropertiesAsync();
    Task<PropertyDto?> GetPropertyByIdAsync(int id);
    Task<IEnumerable<PropertyDto>> GetFeaturedPropertiesAsync(int count = 6);
    Task<IEnumerable<PropertyDto>> GetLatestPropertiesAsync(int count = 8);
    Task<IEnumerable<PropertyDto>> GetPropertiesByUserAsync(string userId);
    Task<IEnumerable<PropertyDto>> SearchPropertiesAsync(PropertySearchDto searchDto);
    Task<PropertyDto> CreatePropertyAsync(CreatePropertyDto dto, string userId);
    Task<PropertyDto> UpdatePropertyAsync(UpdatePropertyDto dto);
    Task DeletePropertyAsync(int id);
    Task<bool> ToggleFeaturedAsync(int id);
    Task<bool> UpdateStatusAsync(int id, string status);
}
