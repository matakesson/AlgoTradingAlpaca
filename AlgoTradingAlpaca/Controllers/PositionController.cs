using AlgoTradingAlpaca.Interfaces;
using AlgoTradingAlpaca.Models;
using Microsoft.AspNetCore.Mvc;

namespace AlgoTradingAlpaca.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PositionController : Controller
{
    
    private readonly IPositionDataService _positionDataService;
    private readonly IAccountDataService _accountDataService;

    public PositionController(IPositionDataService positionDataService, IAccountDataService accountDataService)
    {
        _positionDataService = positionDataService;
        _accountDataService = accountDataService;
    }
    
    [HttpGet("GetPositions")]
    public async Task<ActionResult<List<Position>>> GetPositions()
    {
        var response = await _positionDataService.GetPositionsAsync();
        return Ok(response);
    }

    [HttpGet("GetClosedPositions")]
    public async Task<ActionResult<List<Position>>> GetClosedPositions()
    {
        var response = await _positionDataService.GetClosedPostionsAsync();
        var response2 = await _accountDataService.GetAccountHistoryAsync();
        return Ok(response2);
    }
    
}