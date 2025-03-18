using AlgoTradingAlpaca.Models;

namespace AlgoTradingAlpaca.Interfaces;

public interface ITradingClientService
{
    Task<OrderResponse> PlaceMarketOrderAsync(string symbol, int quantity, string side);
    Task<OrderResponse> CloseOrderAsync(string symbol, int percentage);

}