namespace RealEstateMarketplace.BLL.DTOs;

public class PropertyDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public decimal SquareFeet { get; set; }
    public int? YearBuilt { get; set; }
    public string PropertyType { get; set; } = string.Empty;
    public string ListingType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? MainImageUrl { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime CreatedAt { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? AgentName { get; set; }
    public string? AgentEmail { get; set; }
    public string? AgentPhone { get; set; }
    public string? CategoryName { get; set; }
    public int? CategoryId { get; set; }
    public List<string> ImageUrls { get; set; } = new();
    public List<string> Amenities { get; set; } = new();
    public List<int> AmenityIds { get; set; } = new();
}

public class CreatePropertyDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public decimal SquareFeet { get; set; }
    public int? YearBuilt { get; set; }
    public string PropertyType { get; set; } = string.Empty;
    public string ListingType { get; set; } = string.Empty;
    public string? MainImageUrl { get; set; }
    public int? CategoryId { get; set; }
    public List<int> AmenityIds { get; set; } = new();
}

public class UpdatePropertyDto : CreatePropertyDto
{
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsFeatured { get; set; }
}

public class PropertySearchDto
{
    public string? Keyword { get; set; }
    public string? PropertyType { get; set; }
    public string? ListingType { get; set; }
    public string? City { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? MinBedrooms { get; set; }
    public int? MaxBedrooms { get; set; }
}
