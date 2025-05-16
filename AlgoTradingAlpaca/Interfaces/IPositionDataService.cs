using AlgoTradingAlpaca.Models;

namespace AlgoTradingAlpaca.Interfaces;

public interface IPositionDataService
{
    Task<List<Position>> GetPositionsAsync();
    Task<List<Position>> GetClosedPostionsAsync();
    Task AddPositionAsync(Position position);
    Task UpdatePositionAsync(Position position);
}