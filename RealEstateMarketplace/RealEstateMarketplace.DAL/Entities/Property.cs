using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateMarketplace.DAL.Entities;

public class Property
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string State { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(20)]
    public string ZipCode { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Country { get; set; } = string.Empty;
    
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    
    [Column(TypeName = "decimal(10,2)")]
    public decimal SquareFeet { get; set; }
    
    public int? YearBuilt { get; set; }
    
    public PropertyType PropertyType { get; set; }
    public ListingType ListingType { get; set; }
    public PropertyStatus Status { get; set; } = PropertyStatus.Pending;
    
    public string? MainImageUrl { get; set; }
    
    public bool IsFeatured { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Foreign keys
    [Required]
    public string UserId { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Category? Category { get; set; }
    public virtual ICollection<PropertyImage> Images { get; set; } = new List<PropertyImage>();
    public virtual ICollection<PropertyAmenity> PropertyAmenities { get; set; } = new List<PropertyAmenity>();
    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public virtual ICollection<Inquiry> Inquiries { get; set; } = new List<Inquiry>();
}

public enum PropertyType
{
    House,
    Apartment,
    Condo,
    Townhouse,
    Villa,
    Land,
    Commercial,
    Other
}

public enum ListingType
{
    Sale,
    Rent
}

public enum PropertyStatus
{
    Pending,
    Active,
    Sold,
    Rented,
    Inactive
}
