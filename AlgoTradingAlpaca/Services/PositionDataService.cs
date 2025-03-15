using AlgoTradingAlpaca.Data;
using AlgoTradingAlpaca.Models;

namespace AlgoTradingAlpaca.Services;

public class PositionDataService
{
    private AppDbContext _context;

    public PositionDataService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Position>> GetPositionsAsync()
    {
        var positions = _context.Positions.ToList();
        return positions;
    }
}