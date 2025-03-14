using AlgoTradingAlpaca.Interfaces;
using AlgoTradingAlpaca.Services;
using Microsoft.AspNetCore.Mvc;

namespace AlgoTradingAlpaca.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebSocketController : Controller
{
    private readonly IWebSocketService _webSocketService;

    public WebSocketController(IWebSocketService webSocketService)
    {
        _webSocketService = webSocketService;
    }

    [HttpPost("bars")]
    public async Task<IActionResult> StartBarsWebSocketAsync([FromBody] string[] symbols)
    {
        if (symbols == null || symbols.Length == 0)
        {
            return BadRequest();
        }
        
        await _webSocketService.StartBarsWebSocketAsync();
        return Ok($"Bars WebSocket streaming started for: {string.Join(", ", symbols)}");
    }
}