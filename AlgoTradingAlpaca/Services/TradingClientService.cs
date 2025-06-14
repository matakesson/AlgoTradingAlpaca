using System.Net;
using System.Text;
using System.Text.Json;
using AlgoTradingAlpaca.Configurations;
using AlgoTradingAlpaca.Interfaces;
using AlgoTradingAlpaca.Models;
using Microsoft.Extensions.Options;

namespace AlgoTradingAlpaca.Services;

public class TradingClientService : ITradingClientService
{
    private readonly HttpClient _httpClient;
    private readonly AlpacaConfig _config;

    public TradingClientService(IOptions<AlpacaConfig> configOptions, HttpClient httpClient)
    {
        _config = configOptions.Value ?? throw new ArgumentNullException(nameof(configOptions));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        
        _httpClient.BaseAddress = new Uri(_config.BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("APCA-API-KEY-ID", _config.ApiKey);
        _httpClient.DefaultRequestHeaders.Add("APCA-API-SECRET-KEY", _config.ApiSecret);
    }

    public async Task<OrderResponse> PlaceMarketOrderAsync(string symbol, int quantity, string side, double entryPrice, double takeProfit, double stopLoss)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity));
        }

        if (side.ToLower() != "buy" && side.ToLower() != "sell")
        {
            throw new ArgumentOutOfRangeException("Side must be 'buy' or 'sell'", nameof(side));
        }

        var order = new
        {
            symbol = symbol.ToUpper(),
            qty = quantity,
            side = side.ToLower(),
            type = "market",
            time_in_force = "gtc",
            order_class = "bracket",
            take_profit = new { limit_price = takeProfit },
            stop_loss = new { stop_price = stopLoss }
        };
        
        var content = new StringContent(JsonSerializer.Serialize(order), Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync("orders", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"Raw API response: {responseBody}");

            if (response.IsSuccessStatusCode)
            {
                var deserializedResponse = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<OrderResponse>(deserializedResponse);
            }

            else
            {
                string error = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    throw new Exception("Too many requests");
                }
                throw new Exception($"Error placing order: {error}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    public async Task<OrderResponse> CloseOrderAsync(string symbol, int percentage)
    {
        if (percentage <= 0 || percentage > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(percentage), "Percentage must be between 1 and 100.");
        }
        
        string url = $"positions/{symbol}?percentage={percentage}"; 

        try
        {
            var response = await _httpClient.DeleteAsync(url); 
            var responseBody = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"Raw API response: {responseBody}");

            if (response.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<OrderResponse>(responseBody);
            }
            else
            {
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    throw new Exception("Too many requests. Please wait before trying again.");
                }

                throw new Exception($"Error closing position: {responseBody}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            throw;
        }
    }
    
}