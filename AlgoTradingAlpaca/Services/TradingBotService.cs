using AlgoTradingAlpaca.Interfaces;

namespace AlgoTradingAlpaca.Services;

public class TradingBotService : BackgroundService, ITradingBotService
{
    private bool _isTradingEnabled;
    private readonly IServiceProvider _serviceProvider;

    public bool IsTradingEnabled => _isTradingEnabled;
    
    public TradingBotService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
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

                using (var scope = _serviceProvider.CreateScope())
                {
                    var tradingStrategy = scope.ServiceProvider.GetRequiredService<ITradingStrategy>();

                    await tradingStrategy.ExecuteTradingStrategy();
                }
                
            }
            else
            {
                Console.WriteLine("Trading is currently disabled.");
            }
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}