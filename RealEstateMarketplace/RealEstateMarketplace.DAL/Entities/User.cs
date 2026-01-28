using Microsoft.AspNetCore.Identity;

namespace RealEstateMarketplace.DAL.Entities;

public class User : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;    public bool IsSuperAdmin { get; set; } = false;    
    // Navigation properties
    public virtual ICollection<Property> Properties { get; set; } = new List<Property>();
    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public virtual ICollection<Inquiry> SentInquiries { get; set; } = new List<Inquiry>();
    public virtual ICollection<Inquiry> ReceivedInquiries { get; set; } = new List<Inquiry>();
}
