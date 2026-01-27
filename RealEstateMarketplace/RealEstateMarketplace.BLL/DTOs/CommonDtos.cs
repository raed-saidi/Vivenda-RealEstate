namespace RealEstateMarketplace.BLL.DTOs;

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconClass { get; set; }
    public bool IsActive { get; set; }
    public int PropertyCount { get; set; }
}

public class AmenityDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? IconClass { get; set; }
    public bool IsActive { get; set; }
}

public class InquiryDto
{
    public int Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? SenderName { get; set; }
    public string? SenderEmail { get; set; }
    public string? SenderPhone { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public int PropertyId { get; set; }
    public string PropertyTitle { get; set; } = string.Empty;
}

public class CreateInquiryDto
{
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? SenderName { get; set; }
    public string? SenderEmail { get; set; }
    public string? SenderPhone { get; set; }
    public int PropertyId { get; set; }
}

public class DashboardStatsDto
{
    public int TotalProperties { get; set; }
    public int ActiveProperties { get; set; }
    public int PendingProperties { get; set; }
    public int SoldProperties { get; set; }
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalInquiries { get; set; }
    public int UnreadInquiries { get; set; }
    public int PropertiesForSale { get; set; }
    public int PropertiesForRent { get; set; }
    public int TotalCategories { get; set; }
    public int TotalFavorites { get; set; }
    public List<PropertyDto> RecentProperties { get; set; } = new();
    public List<InquiryDto> RecentInquiries { get; set; } = new();
    public List<CategoryStatsDto> CategoryStats { get; set; } = new();
    public List<MonthlyStatsDto> MonthlyStats { get; set; } = new();
}

public class CategoryStatsDto
{
    public string CategoryName { get; set; } = string.Empty;
    public int PropertyCount { get; set; }
    public string Color { get; set; } = "#007bff";
}

public class MonthlyStatsDto
{
    public string Month { get; set; } = string.Empty;
    public int PropertiesAdded { get; set; }
    public int InquiriesReceived { get; set; }
    public int UsersRegistered { get; set; }
}

public class SettingsDto
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = "General";
    public DateTime UpdatedAt { get; set; }
}
