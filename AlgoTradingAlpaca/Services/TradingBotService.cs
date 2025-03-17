using AlgoTradingAlpaca.Interfaces;

namespace AlgoTradingAlpaca.Services;

public class TradingBotService : BackgroundService, ITradingBotService
{
    private readonly ITradingStrategy _tradingStrategy;
    private bool _isTradingEnabled;

    public bool IsTradingEnabled => _isTradingEnabled;
    
    public TradingBotService(ITradingStrategy tradingStrategy)
    {
        _tradingStrategy = tradingStrategy;
        _isTradingEnabled = false;
    }

    public Task StartTradingAsync()
    {
        _isTradingEnabled = true;
        Console.WriteLine("Trading enabled.");
        return Task.CompletedTask;
    }

    public Task StopTradingAsync()
    {
        _isTradingEnabled = false;
        Console.WriteLine("Trading disabled.");
        return Task.CompletedTask;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Trading Bot Service started.");
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_isTradingEnabled)
            {
                Console.WriteLine("Executing trading strategy...");
                await _tradingStrategy.ExecuteTradingStrategy();
            }
            else
            {
                Console.WriteLine("Trading is currently disabled.");
            }
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}