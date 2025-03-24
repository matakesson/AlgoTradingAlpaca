using System.Text.Json;
using AlgoTradingAlpaca.Data;
using AlgoTradingAlpaca.Interfaces;
using AlgoTradingAlpaca.Models;
using Microsoft.EntityFrameworkCore;

namespace AlgoTradingAlpaca.Services;

public class BarDataService : IBarDataService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly AppDbContext _context;
    private readonly List<BarData> _barList;
    public BarDataService(IServiceProvider serviceProvider, AppDbContext context)
    {
        _serviceProvider = serviceProvider;
        _context = context;
        _barList = new List<BarData>();
    }

    public async Task<List<BarData>> GetBarDataAsync()
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            return context.BarData.ToList();
        }
    }

    public async Task ProcessBarData(JsonElement element)
    {
        if (element.TryGetProperty("S", out var symbolElement) &&
            element.TryGetProperty("o", out var openElement) &&
            element.TryGetProperty("h", out var highElement) &&
            element.TryGetProperty("l", out var lowElement) &&
            element.TryGetProperty("c", out var closeElement) &&
            element.TryGetProperty("t", out var timeElement) &&
            element.TryGetProperty("v", out var volumeElement))
        {
            var barData = new BarData()
            {
                Symbol = symbolElement.GetString(),
                OpenPrice = openElement.GetDouble(),
                HighPrice = highElement.GetDouble(),
                LowPrice = lowElement.GetDouble(),
                ClosingPrice = closeElement.GetDouble(),
                TimeStamp = DateTime.Parse(timeElement.GetString()),
                Volume = volumeElement.GetDouble()
            };

            _barList.Add(barData);

            
            _context.Add(barData);
            await _context.SaveChangesAsync();
                
            _barList.Clear();
            
            
                    
        } 
    }
}