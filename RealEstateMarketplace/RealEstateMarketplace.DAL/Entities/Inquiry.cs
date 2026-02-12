using System.ComponentModel.DataAnnotations;

namespace RealEstateMarketplace.DAL.Entities;

public class Inquiry
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Subject { get; set; } = string.Empty;
    
    [Required]
    public string Message { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? SenderName { get; set; }
    
    [MaxLength(100)]
    public string? SenderEmail { get; set; }
    
    [MaxLength(20)]
    public string? SenderPhone { get; set; }
    
    public bool IsRead { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Foreign keys
    public string? SenderId { get; set; }
    
    [Required]
    public string ReceiverId { get; set; } = string.Empty;
    
    public int PropertyId { get; set; }
    
    // Navigation properties
    public virtual User? Sender { get; set; }
    public virtual User Receiver { get; set; } = null!;
    public virtual Property Property { get; set; } = null!;
}
