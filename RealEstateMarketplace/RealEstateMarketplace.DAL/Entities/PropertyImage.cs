using System.ComponentModel.DataAnnotations;

namespace RealEstateMarketplace.DAL.Entities;

public class PropertyImage
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string ImageUrl { get; set; } = string.Empty;
    
    public string? Caption { get; set; }
    
    public int DisplayOrder { get; set; } = 0;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Foreign key
    public int PropertyId { get; set; }
    
    // Navigation property
    public virtual Property Property { get; set; } = null!;
}
