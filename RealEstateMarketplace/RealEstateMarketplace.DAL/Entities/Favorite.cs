using System.ComponentModel.DataAnnotations;

namespace RealEstateMarketplace.DAL.Entities;

public class Favorite
{
    [Key]
    public int Id { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Foreign keys
    [Required]
    public string UserId { get; set; } = string.Empty;
    public int PropertyId { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Property Property { get; set; } = null!;
}
