using System.ComponentModel.DataAnnotations;

namespace RealEstateMarketplace.DAL.Entities;

public class PropertyAmenity
{
    public int PropertyId { get; set; }
    public int AmenityId { get; set; }
    
    // Navigation properties
    public virtual Property Property { get; set; } = null!;
    public virtual Amenity Amenity { get; set; } = null!;
}
