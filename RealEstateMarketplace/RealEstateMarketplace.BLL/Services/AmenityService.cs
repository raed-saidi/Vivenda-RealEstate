using RealEstateMarketplace.BLL.DTOs;
using RealEstateMarketplace.DAL.Entities;
using RealEstateMarketplace.DAL.Repositories;

namespace RealEstateMarketplace.BLL.Services;

public class AmenityService : IAmenityService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public AmenityService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public async Task<IEnumerable<AmenityDto>> GetAllAmenitiesAsync()
    {
        var amenities = await _unitOfWork.Repository<Amenity>().FindAsync(a => a.IsActive);
        return amenities.Select(a => new AmenityDto
        {
            Id = a.Id,
            Name = a.Name,
            IconClass = a.IconClass,
            IsActive = a.IsActive
        });
    }
    
    public async Task<AmenityDto?> GetAmenityByIdAsync(int id)
    {
        var amenity = await _unitOfWork.Repository<Amenity>().GetByIdAsync(id);
        if (amenity == null) return null;
        
        return new AmenityDto
        {
            Id = amenity.Id,
            Name = amenity.Name,
            IconClass = amenity.IconClass,
            IsActive = amenity.IsActive
        };
    }
}
