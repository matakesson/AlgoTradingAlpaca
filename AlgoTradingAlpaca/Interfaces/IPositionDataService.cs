using AlgoTradingAlpaca.Models;

namespace AlgoTradingAlpaca.Interfaces;

public interface IPositionDataService
{
    Task<List<Position>> GetPositionsAsync();
}