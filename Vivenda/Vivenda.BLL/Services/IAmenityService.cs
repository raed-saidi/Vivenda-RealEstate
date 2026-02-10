using Vivenda.BLL.DTOs;

namespace Vivenda.BLL.Services;

public interface IAmenityService
{
    Task<IEnumerable<AmenityDto>> GetAllAmenitiesAsync();
    Task<AmenityDto?> GetAmenityByIdAsync(int id);
}

