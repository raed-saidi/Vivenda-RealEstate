using RealEstateMarketplace.BLL.DTOs;

namespace RealEstateMarketplace.BLL.Services;

public interface IChatbotService
{
    Task<ChatbotResponseDto> ProcessMessageAsync(string message, string? context = null);
}
