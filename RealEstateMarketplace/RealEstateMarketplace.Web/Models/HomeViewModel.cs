using RealEstateMarketplace.BLL.DTOs;

namespace RealEstateMarketplace.Web.Models;

public class HomeViewModel
{
    public List<PropertyDto> FeaturedProperties { get; set; } = new();
    public List<PropertyDto> LatestProperties { get; set; } = new();
    public List<CategoryDto> Categories { get; set; } = new();
}

public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
