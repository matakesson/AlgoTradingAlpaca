using AlgoTradingAlpaca.Configurations;

namespace AlgoTradingAlpaca.Interfaces;

public interface IWebSocketService
{
    Task StartBarsWebSocketAsync(string[] symbols);
}