using RealEstateMarketplace.BLL.DTOs;

namespace RealEstateMarketplace.BLL.Services;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetDashboardStatsAsync();
}
