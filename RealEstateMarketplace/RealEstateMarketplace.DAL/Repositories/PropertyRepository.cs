using Microsoft.EntityFrameworkCore;
using RealEstateMarketplace.DAL.Data;
using RealEstateMarketplace.DAL.Entities;

namespace RealEstateMarketplace.DAL.Repositories;

public class PropertyRepository : Repository<Property>, IPropertyRepository
{
    public PropertyRepository(ApplicationDbContext context) : base(context)
    {
    }
    
    public async Task<IEnumerable<Property>> GetPropertiesWithDetailsAsync()
    {
        return await _dbSet
            .Include(p => p.User)
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.PropertyAmenities)
                .ThenInclude(pa => pa.Amenity)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }
    
    public async Task<Property?> GetPropertyWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(p => p.User)
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.PropertyAmenities)
                .ThenInclude(pa => pa.Amenity)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
    
    public async Task<IEnumerable<Property>> GetFeaturedPropertiesAsync(int count = 6)
    {
        return await _dbSet
            .Include(p => p.User)
            .Include(p => p.Category)
            .Where(p => p.IsFeatured && p.Status == PropertyStatus.Active)
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Property>> GetLatestPropertiesAsync(int count = 8)
    {
        return await _dbSet
            .Include(p => p.User)
            .Include(p => p.Category)
            .Where(p => p.Status == PropertyStatus.Active)
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Property>> GetPropertiesByUserAsync(string userId)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Property>> SearchPropertiesAsync(
        string? keyword = null,
        PropertyType? propertyType = null,
        ListingType? listingType = null,
        string? city = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int? minBedrooms = null,
        int? maxBedrooms = null)
    {
        var query = _dbSet
            .Include(p => p.User)
            .Include(p => p.Category)
            .Where(p => p.Status == PropertyStatus.Active)
            .AsQueryable();
            
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(p => 
                p.Title.Contains(keyword) || 
                p.Description.Contains(keyword) || 
                p.Address.Contains(keyword));
        }
        
        if (propertyType.HasValue)
            query = query.Where(p => p.PropertyType == propertyType.Value);
            
        if (listingType.HasValue)
            query = query.Where(p => p.ListingType == listingType.Value);
            
        if (!string.IsNullOrWhiteSpace(city))
            query = query.Where(p => p.City.Contains(city));
            
        if (minPrice.HasValue)
            query = query.Where(p => p.Price >= minPrice.Value);
            
        if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice.Value);
            
        if (minBedrooms.HasValue)
            query = query.Where(p => p.Bedrooms >= minBedrooms.Value);
            
        if (maxBedrooms.HasValue)
            query = query.Where(p => p.Bedrooms <= maxBedrooms.Value);
            
        return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
    }
}
