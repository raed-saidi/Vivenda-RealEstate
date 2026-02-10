using Microsoft.AspNetCore.Mvc;
using Vivenda.BLL.DTOs;
using Vivenda.BLL.Services;

namespace Vivenda.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatbotController : ControllerBase
{
    private readonly IChatbotService _chatbotService;

    public ChatbotController(IChatbotService chatbotService)
    {
        _chatbotService = chatbotService;
    }

    [HttpPost("message")]
    public async Task<IActionResult> SendMessage([FromBody] ChatbotMessageDto message)
    {
        if (string.IsNullOrWhiteSpace(message.Message))
        {
            return BadRequest(new { error = "Message cannot be empty" });
        }

        var response = await _chatbotService.ProcessMessageAsync(message.Message, message.Context);
        return Ok(response);
    }
}

