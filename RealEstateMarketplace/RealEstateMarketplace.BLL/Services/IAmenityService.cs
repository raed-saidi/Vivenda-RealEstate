using RealEstateMarketplace.BLL.DTOs;

namespace RealEstateMarketplace.BLL.Services;

public interface IAmenityService
{
    Task<IEnumerable<AmenityDto>> GetAllAmenitiesAsync();
    Task<AmenityDto?> GetAmenityByIdAsync(int id);
}
