using AlgoTradingAlpaca.Models;

namespace AlgoTradingAlpaca.Interfaces;

public interface IPositionDataService
{
    Task<List<Position>> GetPositionsAsync();
    Task AddPositionAsync(Position position);
    Task UpdatePositionAsync(Position position);
}