using AlgoTradingAlpaca.Models;

namespace AlgoTradingAlpaca.Interfaces;

public interface IBarDataService
{
    Task<List<BarData>> GetBarDataAsync();
}