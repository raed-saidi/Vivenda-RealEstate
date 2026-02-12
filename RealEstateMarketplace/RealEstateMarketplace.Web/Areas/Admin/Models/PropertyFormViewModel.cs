using System.ComponentModel.DataAnnotations;

namespace RealEstateMarketplace.Web.Areas.Admin.Models;

public class PropertyFormViewModel
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [Range(0, double.MaxValue)]
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
    
    [Range(0, 50)]
    public int Bedrooms { get; set; }
    
    [Range(0, 50)]
    public int Bathrooms { get; set; }
    
    [Range(0, 100000)]
    public decimal SquareFeet { get; set; }
    
    public int? YearBuilt { get; set; }
    
    [Required]
    public string PropertyType { get; set; } = "House";
    
    [Required]
    public string ListingType { get; set; } = "Sale";
    
    public string Status { get; set; } = "Pending";
    
    public string? MainImageUrl { get; set; }
    
    public bool IsFeatured { get; set; }
    
    public int? CategoryId { get; set; }
    
    public List<int>? SelectedAmenityIds { get; set; } = new();
}
