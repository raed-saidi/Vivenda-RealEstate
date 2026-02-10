using Vivenda.BLL.DTOs;

namespace Vivenda.BLL.Services;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetDashboardStatsAsync();
}

