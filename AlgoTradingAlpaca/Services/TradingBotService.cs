using AlgoTradingAlpaca.Interfaces;

namespace AlgoTradingAlpaca.Services;

public class TradingBotService : BackgroundService, ITradingBotService
{
    private bool _isTradingEnabled;
    private bool _isReconnecting;
    private readonly IServiceProvider _serviceProvider;

    public bool IsTradingEnabled => _isTradingEnabled;
    public bool IsReconnecting => _isReconnecting;
    
    public TradingBotService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _isTradingEnabled = false;
        _isReconnecting = false;
    }

    public Task StartTradingAsync()
    {
        _isReconnecting = true;
        _ = Task.Run(async () =>
        {
            while (_isReconnecting)
            {
                var now = DateTime.Now;
                var marketOpen = new TimeSpan(15, 30, 0);
                var marketClose = new TimeSpan(22, 0, 0);
                bool isWeekday = now.DayOfWeek >= DayOfWeek.Monday && now.DayOfWeek <= DayOfWeek.Friday;
                bool isOpen = now.TimeOfDay >= marketOpen && now.TimeOfDay <= marketClose;

                if (isWeekday && isOpen)
                {
                    _isTradingEnabled = true;
                    _isReconnecting = false;
                    Console.WriteLine("Trading enabled.");
                    break;
                }

                Console.WriteLine("Waiting for market to open... rechecking in 1 minute.");
                await Task.Delay(TimeSpan.FromMinutes(1));
            }
        });

        return Task.CompletedTask;
    }

    public Task StopTradingAsync()
    {
        _isTradingEnabled = false;
        _isReconnecting = false;
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