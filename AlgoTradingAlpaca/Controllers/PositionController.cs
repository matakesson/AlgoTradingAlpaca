using AlgoTradingAlpaca.Interfaces;
using AlgoTradingAlpaca.Models;
using Microsoft.AspNetCore.Mvc;

namespace AlgoTradingAlpaca.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PositionController : Controller
{
    
    private readonly IPositionDataService _positionDataService;

    public PositionController(IPositionDataService positionDataService)
    {
        _positionDataService = positionDataService;
    }
    
    [HttpGet("GetPositions")]
    public async Task<ActionResult<List<Position>>> GetPositions()
    {
        var response = await _positionDataService.GetPositionsAsync();
        return Ok(response);
    }
}