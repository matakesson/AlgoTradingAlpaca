using AlgoTradingAlpaca.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AlgoTradingAlpaca.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TradingBotController : Controller
{
    private readonly ITradingBotService _tradingBotService;

    public TradingBotController(ITradingBotService tradingBotService)
    {
        _tradingBotService = tradingBotService;
    }

    [HttpPost("Start")]
    public async Task<IActionResult> StartBot()
    {
        await _tradingBotService.StartTradingAsync();
        return Ok("Trading bot started");
    }

    [HttpPost("Stop")]
    public async Task<IActionResult> StopBot()
    {
        await _tradingBotService.StopTradingAsync();
        return Ok("Trading bot stopped");
    }
}