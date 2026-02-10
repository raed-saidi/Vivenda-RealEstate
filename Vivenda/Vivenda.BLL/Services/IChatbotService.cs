using Vivenda.BLL.DTOs;

namespace Vivenda.BLL.Services;

public interface IChatbotService
{
    Task<ChatbotResponseDto> ProcessMessageAsync(string message, string? context = null);
}

