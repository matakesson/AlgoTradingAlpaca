using AlgoTradingAlpaca.Models;

namespace AlgoTradingAlpaca.Interfaces;

public interface ITradingClientService
{
    Task<OrderResponse> PlaceMarketOrderAsync(string symbol, int quantity, string side, double entryPrice, double takeProfit, double stopLoss);
    Task<OrderResponse> CloseOrderAsync(string symbol, int percentage);

}