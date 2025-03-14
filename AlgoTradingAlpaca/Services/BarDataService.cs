using AlgoTradingAlpaca.Data;
using AlgoTradingAlpaca.Models;
using Microsoft.EntityFrameworkCore;

namespace AlgoTradingAlpaca.Services;

public class BarDataService
{
    private AppDbContext _context;

    public BarDataService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<BarData>> GetBarData()
    {
        var barData = _context.BarData.ToList();
        return barData;
    }
}