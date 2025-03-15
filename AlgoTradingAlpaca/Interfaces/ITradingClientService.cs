namespace AlgoTradingAlpaca.Interfaces;

public interface ITradingClientService
{
    Task PlaceMarketOrderAsync(string symbol, int quantity, string side, double currentPrice, double takeProfit, double stopLoss);
}