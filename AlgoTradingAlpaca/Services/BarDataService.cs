using AlgoTradingAlpaca.Data;
using AlgoTradingAlpaca.Interfaces;
using AlgoTradingAlpaca.Models;
using Microsoft.EntityFrameworkCore;

namespace AlgoTradingAlpaca.Services;

public class BarDataService : IBarDataService
{
    private readonly IServiceProvider _serviceProvider;
    public BarDataService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<List<BarData>> GetBarDataAsync()
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            return context.BarData.ToList();
        }
    }
}