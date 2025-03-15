using AlgoTradingAlpaca.Models;

namespace AlgoTradingAlpaca.Interfaces;

public interface ITradingClientService
{
    Task<OrderResponse> PlaceMarketOrderAsync(string symbol, int quantity, string side, double currentPrice, double takeProfit, double stopLoss);
}