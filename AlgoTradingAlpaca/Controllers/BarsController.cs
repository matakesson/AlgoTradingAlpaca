using AlgoTradingAlpaca.Interfaces;
using AlgoTradingAlpaca.Models;
using Microsoft.AspNetCore.Mvc;

namespace AlgoTradingAlpaca.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BarsController : Controller
{
    private readonly IBarDataService _barDataService;

    public BarsController(IBarDataService barDataService)
    {
        _barDataService = barDataService;
    }
    
    [HttpGet("GetBars/{symbol}")]
    public async Task<ActionResult<List<BarData>>> GetBars(string symbol)
    {
        var response = await _barDataService.GetBars(symbol);
        return Ok(response);
    }
}