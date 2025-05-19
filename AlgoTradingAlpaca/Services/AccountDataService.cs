using System.Text.Json;
using AlgoTradingAlpaca.Configurations;
using AlgoTradingAlpaca.Interfaces;
using AlgoTradingAlpaca.Models;
using Microsoft.Extensions.Options;

namespace AlgoTradingAlpaca.Services;

public class AccountDataService : IAccountDataService
{
    private readonly HttpClient _httpClient;
    private readonly AlpacaConfig _config;

    public AccountDataService(IOptions<AlpacaConfig> configOptions, HttpClient httpClient)
    {
        _config = configOptions.Value ?? throw new ArgumentNullException(nameof(configOptions));
        _httpClient = httpClient;

        _httpClient.BaseAddress = new Uri(_config.BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("APCA-API-KEY-ID", _config.ApiKey);
        _httpClient.DefaultRequestHeaders.Add("APCA-API-SECRET-KEY", _config.ApiSecret);
    }

    public async Task<Account> GetAccountAsync()
    {
        Console.WriteLine($"Calling {_httpClient.BaseAddress}account");
        var response = await _httpClient.GetAsync("account");
        response.EnsureSuccessStatusCode();
        var jsonResponse = await response.Content.ReadAsStringAsync();
        var account = JsonSerializer.Deserialize<Account>(jsonResponse);

        return account;
    }

    public async Task<AccountHistory> GetAccountHistoryAsync()
    {
        
        var response = await _httpClient.GetAsync("account/portfolio/history");
        Console.WriteLine(response);
        response.EnsureSuccessStatusCode();
        var jsonResponse = await response.Content.ReadAsStringAsync();
        var accountHistory = JsonSerializer.Deserialize<AccountHistory>(jsonResponse);
        Console.WriteLine(accountHistory);
        return accountHistory;
    }
}