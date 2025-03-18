using System.Text.Json;
using AlgoTradingAlpaca.Models;

namespace AlgoTradingAlpaca.Interfaces;

public interface IBarDataService
{
    Task<List<BarData>> GetBarDataAsync();
    Task ProcessBarData(JsonElement element);
}