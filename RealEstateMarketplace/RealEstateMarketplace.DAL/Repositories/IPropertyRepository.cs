using RealEstateMarketplace.DAL.Entities;

namespace RealEstateMarketplace.DAL.Repositories;

public interface IPropertyRepository : IRepository<Property>
{
    Task<IEnumerable<Property>> GetPropertiesWithDetailsAsync();
    Task<Property?> GetPropertyWithDetailsAsync(int id);
    Task<IEnumerable<Property>> GetFeaturedPropertiesAsync(int count = 6);
    Task<IEnumerable<Property>> GetLatestPropertiesAsync(int count = 8);
    Task<IEnumerable<Property>> GetPropertiesByUserAsync(string userId);
    Task<IEnumerable<Property>> SearchPropertiesAsync(
        string? keyword = null,
        PropertyType? propertyType = null,
        ListingType? listingType = null,
        string? city = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int? minBedrooms = null,
        int? maxBedrooms = null);
}
