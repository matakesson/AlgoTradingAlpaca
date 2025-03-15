using AlgoTradingAlpaca.Data;
using AlgoTradingAlpaca.Interfaces;
using AlgoTradingAlpaca.Models;
using Microsoft.EntityFrameworkCore;

namespace AlgoTradingAlpaca.Services;

public class BarDataService : IBarDataService
{
    private AppDbContext _context;

    public BarDataService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<BarData>> GetBarDataAsync()
    {
        var barData = _context.BarData.ToList();
        return barData;
    }
}