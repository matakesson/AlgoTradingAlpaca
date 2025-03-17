namespace AlgoTradingAlpaca.Interfaces;

public interface ITradingBotService
{
    Task StartTradingAsync();
    Task StopTradingAsync();
    bool IsTradingEnabled { get; }
}