using RealEstateMarketplace.BLL.DTOs;
using RealEstateMarketplace.DAL.Entities;
using RealEstateMarketplace.DAL.Repositories;

namespace RealEstateMarketplace.BLL.Services;

public class PropertyService : IPropertyService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public PropertyService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public async Task<IEnumerable<PropertyDto>> GetAllPropertiesAsync()
    {
        var properties = await _unitOfWork.Properties.GetPropertiesWithDetailsAsync();
        return properties.Select(MapToDto);
    }
    
    public async Task<PropertyDto?> GetPropertyByIdAsync(int id)
    {
        var property = await _unitOfWork.Properties.GetPropertyWithDetailsAsync(id);
        return property == null ? null : MapToDto(property);
    }
    
    public async Task<IEnumerable<PropertyDto>> GetFeaturedPropertiesAsync(int count = 6)
    {
        var properties = await _unitOfWork.Properties.GetFeaturedPropertiesAsync(count);
        return properties.Select(MapToDto);
    }
    
    public async Task<IEnumerable<PropertyDto>> GetLatestPropertiesAsync(int count = 8)
    {
        var properties = await _unitOfWork.Properties.GetLatestPropertiesAsync(count);
        return properties.Select(MapToDto);
    }
    
    public async Task<IEnumerable<PropertyDto>> GetPropertiesByUserAsync(string userId)
    {
        var properties = await _unitOfWork.Properties.GetPropertiesByUserAsync(userId);
        return properties.Select(MapToDto);
    }
    
    public async Task<IEnumerable<PropertyDto>> SearchPropertiesAsync(PropertySearchDto searchDto)
    {
        PropertyType? propertyType = null;
        ListingType? listingType = null;
        
        if (!string.IsNullOrEmpty(searchDto.PropertyType) && Enum.TryParse<PropertyType>(searchDto.PropertyType, out var pt))
            propertyType = pt;
            
        if (!string.IsNullOrEmpty(searchDto.ListingType) && Enum.TryParse<ListingType>(searchDto.ListingType, out var lt))
            listingType = lt;
            
        var properties = await _unitOfWork.Properties.SearchPropertiesAsync(
            searchDto.Keyword,
            propertyType,
            listingType,
            searchDto.City,
            searchDto.MinPrice,
            searchDto.MaxPrice,
            searchDto.MinBedrooms,
            searchDto.MaxBedrooms);
            
        return properties.Select(MapToDto);
    }
    
    public async Task<PropertyDto> CreatePropertyAsync(CreatePropertyDto dto, string userId)
    {
        var property = new Property
        {
            Title = dto.Title,
            Description = dto.Description,
            Price = dto.Price,
            Address = dto.Address,
            City = dto.City,
            State = dto.State,
            ZipCode = dto.ZipCode,
            Country = dto.Country,
            Bedrooms = dto.Bedrooms,
            Bathrooms = dto.Bathrooms,
            SquareFeet = dto.SquareFeet,
            YearBuilt = dto.YearBuilt,
            PropertyType = Enum.Parse<PropertyType>(dto.PropertyType),
            ListingType = Enum.Parse<ListingType>(dto.ListingType),
            Status = PropertyStatus.Pending,
            MainImageUrl = dto.MainImageUrl,
            CategoryId = dto.CategoryId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };
        
        // Add amenities
        foreach (var amenityId in dto.AmenityIds)
        {
            property.PropertyAmenities.Add(new PropertyAmenity
            {
                AmenityId = amenityId
            });
        }
        
        await _unitOfWork.Properties.AddAsync(property);
        
        var createdProperty = await _unitOfWork.Properties.GetPropertyWithDetailsAsync(property.Id);
        return MapToDto(createdProperty!);
    }
    
    public async Task<PropertyDto> UpdatePropertyAsync(UpdatePropertyDto dto)
    {
        var property = await _unitOfWork.Properties.GetPropertyWithDetailsAsync(dto.Id);
        if (property == null)
            throw new Exception("Property not found");
            
        property.Title = dto.Title;
        property.Description = dto.Description;
        property.Price = dto.Price;
        property.Address = dto.Address;
        property.City = dto.City;
        property.State = dto.State;
        property.ZipCode = dto.ZipCode;
        property.Country = dto.Country;
        property.Bedrooms = dto.Bedrooms;
        property.Bathrooms = dto.Bathrooms;
        property.SquareFeet = dto.SquareFeet;
        property.YearBuilt = dto.YearBuilt;
        property.PropertyType = Enum.Parse<PropertyType>(dto.PropertyType);
        property.ListingType = Enum.Parse<ListingType>(dto.ListingType);
        property.Status = Enum.Parse<PropertyStatus>(dto.Status);
        property.MainImageUrl = dto.MainImageUrl;
        property.CategoryId = dto.CategoryId;
        property.IsFeatured = dto.IsFeatured;
        property.UpdatedAt = DateTime.UtcNow;
        
        // Update amenities
        property.PropertyAmenities.Clear();
        foreach (var amenityId in dto.AmenityIds)
        {
            property.PropertyAmenities.Add(new PropertyAmenity
            {
                PropertyId = property.Id,
                AmenityId = amenityId
            });
        }
        
        await _unitOfWork.Properties.UpdateAsync(property);
        
        var updatedProperty = await _unitOfWork.Properties.GetPropertyWithDetailsAsync(property.Id);
        return MapToDto(updatedProperty!);
    }
    
    public async Task DeletePropertyAsync(int id)
    {
        var property = await _unitOfWork.Properties.GetByIdAsync(id);
        if (property != null)
        {
            await _unitOfWork.Properties.DeleteAsync(property);
        }
    }
    
    public async Task<bool> ToggleFeaturedAsync(int id)
    {
        var property = await _unitOfWork.Properties.GetByIdAsync(id);
        if (property == null)
            return false;
            
        property.IsFeatured = !property.IsFeatured;
        property.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Properties.UpdateAsync(property);
        return property.IsFeatured;
    }
    
    public async Task<bool> UpdateStatusAsync(int id, string status)
    {
        var property = await _unitOfWork.Properties.GetByIdAsync(id);
        if (property == null)
            return false;
            
        property.Status = Enum.Parse<PropertyStatus>(status);
        property.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Properties.UpdateAsync(property);
        return true;
    }
    
    private PropertyDto MapToDto(Property property)
    {
        return new PropertyDto
        {
            Id = property.Id,
            Title = property.Title,
            Description = property.Description,
            Price = property.Price,
            Address = property.Address,
            City = property.City,
            State = property.State,
            ZipCode = property.ZipCode,
            Country = property.Country,
            Bedrooms = property.Bedrooms,
            Bathrooms = property.Bathrooms,
            SquareFeet = property.SquareFeet,
            YearBuilt = property.YearBuilt,
            PropertyType = property.PropertyType.ToString(),
            ListingType = property.ListingType.ToString(),
            Status = property.Status.ToString(),
            MainImageUrl = property.MainImageUrl,
            IsFeatured = property.IsFeatured,
            CreatedAt = property.CreatedAt,
            UserId = property.UserId,
            UserName = property.User != null ? $"{property.User.FirstName} {property.User.LastName}" : "",
            CategoryName = property.Category?.Name,
            CategoryId = property.CategoryId,
            ImageUrls = property.Images?.Select(i => i.ImageUrl).ToList() ?? new List<string>(),
            Amenities = property.PropertyAmenities?.Select(pa => pa.Amenity?.Name ?? "").ToList() ?? new List<string>(),
            AmenityIds = property.PropertyAmenities?.Select(pa => pa.AmenityId).ToList() ?? new List<int>()
        };
    }
}
