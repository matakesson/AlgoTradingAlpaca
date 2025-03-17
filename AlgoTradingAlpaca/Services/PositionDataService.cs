using AlgoTradingAlpaca.Data;
using AlgoTradingAlpaca.Interfaces;
using AlgoTradingAlpaca.Models;

namespace AlgoTradingAlpaca.Services;

public class PositionDataService : IPositionDataService
{
    private IServiceProvider _serviceProvider;

    public PositionDataService(IServiceProvider serviceProvider)
    {
        _serviceProvider  = serviceProvider;
    }

    public async Task<List<Position>> GetPositionsAsync()
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            return context.Positions.ToList();
        }
    }
}