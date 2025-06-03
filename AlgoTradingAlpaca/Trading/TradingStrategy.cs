using System.Globalization;
using AlgoTradingAlpaca.Data;
using AlgoTradingAlpaca.Interfaces;
using AlgoTradingAlpaca.Models;
using AlgoTradingAlpaca.Trading.Helpers;
using AlgoTradingAlpaca.Trading.Strategies;

namespace AlgoTradingAlpaca.Trading;

public class TradingStrategy : ITradingStrategy
{
    private readonly ITradingClientService _tradingClientService;
    private readonly IBarDataService _barDataService;
    private readonly IPositionDataService _positionDataService;
    private readonly IAccountDataService _accountDataService;
    private readonly AppDbContext _dbContext;
    private readonly ICalculateBullishReversal _calculateBullishReversal;
    private readonly ICalculateBearishReversal _calculateBearishReversal;

    public TradingStrategy(ITradingClientService tradingClientService, IBarDataService barDataService, IPositionDataService positionDataService, AppDbContext dbContext,
        IAccountDataService accountDataService)
    {
        _tradingClientService = tradingClientService;
        _barDataService = barDataService;
        _positionDataService = positionDataService;
        _accountDataService = accountDataService;
        _dbContext = dbContext;
        
        _calculateBullishReversal = new CalculateBullishReversal();
        _calculateBearishReversal = new CalculateBearishReversal();
    }

    public async Task ExecuteTradingStrategy()
    {
        Console.WriteLine("[STRATEGY] Starting strategy execution");
        var barData = await _barDataService.GetBarDataAsync();
        Console.WriteLine($"[DATA] Loaded {barData.Count} total bars");

        var groupedAndSortedData = barData
            .GroupBy(bar => bar.Symbol)
            .ToDictionary(
                group => group.Key,
                group => group.OrderBy(bar => bar.TimeStamp).ToList()
            );
    
        var positions = await _positionDataService.GetPositionsAsync();
        var totalOpenPositions = positions.Where(p => p.Status == "Open").ToList();
        Console.WriteLine($"[POSITIONS] Found {totalOpenPositions.Count()} total active positions");

    
        foreach (var kvp in groupedAndSortedData)
        {
            var symbol = kvp.Key;
            var bars = kvp.Value;

            var openPosition = positions.FirstOrDefault(p => p.Symbol == symbol && p.Status == "Open");

            if(openPosition == null)
            {
                var signal = await _calculateBullishReversal.CalculateBullishReversalSignal(bars);
                Console.WriteLine($"[SIGNAL]: {symbol} breakout: {signal}");
                if (signal is { IsBreakout: true })
                {
                    double takeProfit = CalculateTakeProfit(signal.Point1, signal.EntryPrice, "Long");
                    if (takeProfit <= signal.EntryPrice)
                    {
                        double fallbackMove = (signal.EntryPrice - signal.Point1) * 1.618;
                        takeProfit = Math.Round(signal.EntryPrice + fallbackMove, 2);
                    }
                    double rawStopLoss = signal.Point1;
                    double stopBuffer = 0.25;  
                    double maxStopLoss = Math.Round(signal.EntryPrice - stopBuffer, 2);

                    
                    double stopLoss = Math.Min(Math.Round(rawStopLoss, 2), maxStopLoss);
                    Console.WriteLine($"ENTRY: {signal.EntryPrice} STOP LOS: {stopLoss} PROFIT: {takeProfit}");
                    if (stopLoss >= maxStopLoss)
                    {
                        Console.WriteLine($"[ERROR] stopLoss too close to base price. stopLoss={stopLoss}, maxAllowed={maxStopLoss}");
                        continue;
                    }
                    int quantity = await CalculateOrderQuantity(stopLoss, signal.EntryPrice, "Long");
                    if (quantity != 0)
                    {
                        try
                        {
                            await _tradingClientService.PlaceMarketOrderAsync(
                                symbol,
                                quantity,
                                "buy",
                                signal.EntryPrice,
                                takeProfit,
                                stopLoss
                            );

                            var position = new Position
                            {
                                Symbol = symbol,
                                Qty = quantity,
                                AverageEntryPrice = signal.EntryPrice,
                                OpenTime = DateTime.Now,
                                TakeProfit = takeProfit,
                                StopLoss = stopLoss,
                                Status = "Open",
                                Type = "Long"
                            };

                            await _positionDataService.AddPositionAsync(position);
                        }

                        catch (Exception ex)
                        {
                            Console.WriteLine("Error while placing market order" + ex.Message);
                        }
                        
                        
                    }
                }

                var shortSignal = await _calculateBearishReversal.CalculateBearishReversalSignal(barData);
                Console.WriteLine($"[SIGNAL]: {symbol} breakout: {shortSignal}");
                if (shortSignal is { IsBreakout: true })
                {
                    double takeProfit = CalculateTakeProfit(shortSignal.Point1, shortSignal.EntryPrice, "short");
                    double rawStopLoss = shortSignal.Point1;
                    double stopBuffer = 0.25;  
                    double maxStopLoss = Math.Round(shortSignal.EntryPrice + stopBuffer, 2);

                    double stopLoss = Math.Min(Math.Round(rawStopLoss, 2), maxStopLoss);
                    if (stopLoss <= maxStopLoss)
                    {
                        Console.WriteLine($"[ERROR] stopLoss too close to base price. stopLoss={stopLoss}, maxAllowed={maxStopLoss}");
                        continue;
                    }                    int quantity = await CalculateOrderQuantity(stopLoss, shortSignal.EntryPrice, "Short");
                    if (quantity != 0)
                    {
                        try
                        {
                            await _tradingClientService.PlaceMarketOrderAsync(
                                symbol,
                                quantity,
                                "sell",
                                shortSignal.EntryPrice,
                                takeProfit,
                                stopLoss
                            );

                            var position = new Position
                            {
                                Symbol = symbol,
                                Qty = quantity,
                                AverageEntryPrice = shortSignal.EntryPrice,
                                OpenTime = DateTime.Now,
                                TakeProfit = takeProfit,
                                StopLoss = stopLoss,
                                Status = "Open",
                                Type = "Short"
                            };

                            await _positionDataService.AddPositionAsync(position);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error while placing order" + ex.Message);
                        }
                        
                        
                    }
                }
            }
            else
            {
                Console.WriteLine($"[POSITION] {symbol} has open positions: {openPosition}");
                double stopLoss = openPosition.StopLoss;
                double takeProfit = openPosition.TakeProfit;
                double currentHighPrice = bars.Last().HighPrice;
                double currentLowPrice = bars.Last().LowPrice;
                double currentPrice = bars.Last().ClosingPrice;
                if (openPosition.Type == "Long" && currentHighPrice >= takeProfit || openPosition.Type == "Short" && currentLowPrice <= takeProfit)
                {
                    openPosition.Status = "Profit";
                    openPosition.ExitTime = DateTime.Now;
                    openPosition.ClosingPrice = currentPrice;
                    await _positionDataService.UpdatePositionAsync(openPosition);
                }

                if (openPosition.Type == "Long" && currentLowPrice <= stopLoss || openPosition.Type == "Short" && currentHighPrice >= stopLoss)
                {
                    openPosition.Status = "Loss";
                    openPosition.ExitTime = DateTime.Now;
                    openPosition.ClosingPrice = currentPrice;
                    await _positionDataService.UpdatePositionAsync(openPosition);
                }
            }
        }
        Console.WriteLine("[STRATEGY] Execution completed");
    }

    // public async Task<TradeSignalResult?> CalculateBullishReversalSignal(List<BarData> barData)
    // {
    //     const int lookbackPeriod = 60;
    //     const double minDownTrendPercentage = 0;
    //     if (barData.Count < lookbackPeriod)
    //         return null;
    //     var orderedBars = barData.OrderBy(b => b.TimeStamp).ToList();
    //     var recentBars = orderedBars.Skip(Math.Max(0, orderedBars.Count - lookbackPeriod)).ToList();
    //     var lowIndex = LongHelpers.FindLowPointIndex(recentBars);
    //     var lowClose = recentBars[lowIndex].LowPrice;
    //     if (!LongHelpers.IsValidDownTrend(recentBars, lowClose, lowIndex, lookbackPeriod, minDownTrendPercentage))
    //         return null;
    //     if (!LongHelpers.IsValidLowPosition(recentBars, lowIndex))
    //         return null;
    //     var bullishTrend = LongHelpers.BuildBullishTrend(recentBars, lowIndex);
    //     if (bullishTrend.Count == 0)
    //         return null;
    //     var bullishHighIndex = lowIndex + bullishTrend.Count - 1;
    //     if (bullishHighIndex >= recentBars.Count)
    //         return null;
    //     double bullishHigh = bullishTrend.Last().ClosingPrice;
    //     var bearishStart = bullishHighIndex + 1;
    //     if (bearishStart >= recentBars.Count)
    //         return null;
    //     var bearishTrend = LongHelpers.BuildBearishTrend(recentBars, bearishStart);
    //     if (bearishTrend.Count < 3)
    //         return null;
    //     double currentPrice = recentBars.Last().ClosingPrice;
    //     double prevPrice = recentBars[recentBars.Count - 2].ClosingPrice;
    //     bool isBreakout = currentPrice > bullishHigh;
    //     Console.WriteLine(currentPrice);
    //     if (!isBreakout)
    //         return null;
    //     return new TradeSignalResult
    //     {
    //         IsBreakout = true,
    //         EntryPrice = currentPrice,
    //         Point1 = lowClose,
    //         Point2 = bullishHigh
    //     };
    // }
    
    private async Task<TradeSignalResult?> CalculateBearishReversal(List<BarData> barData)
    {
        const int lookbackPeriod = 60;
        const double minUpTrendPercentage = 0;
        if (barData.Count < lookbackPeriod)
            return null;
        var orderedBars = barData.OrderBy(b => b.TimeStamp).ToList();
        var recentBars = orderedBars.Skip(Math.Max(0, orderedBars.Count - lookbackPeriod)).ToList();
        var highIndex = ShortHelpers.FindHighPointIndex(barData);
        int recentOffset = barData.Count - recentBars.Count;
        int recentHighIndex = highIndex - recentOffset;
        if (recentHighIndex < 0 || recentHighIndex >= recentBars.Count)
            return null;
        var highClose = recentBars[recentHighIndex].HighPrice;
        // if (!ShortHelpers.IsValidUpTrend(barData, highClose, highIndex, lookbackPeriod, minUpTrendPercentage)) 
        //     return null;
        if (!ShortHelpers.IsValidHighPosition(barData, highIndex))
            return null;
        var bearishTrend = ShortHelpers.BuildBearishTrend(barData, highIndex);
        if (bearishTrend.Count == 0)
            return null;
        var bearishLowIndex = highIndex + bearishTrend.Count - 1;
        if (bearishLowIndex >= recentBars.Count)
            return null;
        double bearishLow = bearishTrend.Last().LowPrice;
        var bullishStart = bearishLowIndex + 1;
        if (bullishStart >= recentBars.Count)
            return null;
        var bullishTrend = ShortHelpers.BuildBullishTrend(barData, bullishStart);
        if (bullishTrend.Count < 3)
            return null;
        double currentPrice = recentBars.Last().ClosingPrice;
        double prevPrice = recentBars[recentBars.Count - 2].ClosingPrice;
        bool isBreakout = currentPrice < bearishLow;
        if (!isBreakout)
            return null;
        return new TradeSignalResult
        {
            IsBreakout = true,
            EntryPrice = currentPrice,
            Point1 = highClose,
            Point2 = bearishLow
        };
    }
    
    private async Task<int> CalculateOrderQuantity(double stopLossPrice, double entryPrice, string type)
    {
        var account = await _accountDataService.GetAccountAsync();

        int maxPositions = 5;
        int openPositions = _dbContext.Positions.Count(p => p.Status == "Open");
        if (openPositions >= maxPositions)
            return 0;

        
        double buyingPower = Convert.ToDouble(account.buying_power, CultureInfo.InvariantCulture);

        int remainingSlots = maxPositions - openPositions;
        double maxRiskPerTradeFraction = 0.05 / remainingSlots;   
        double maxCapitalPerTradeFraction = 0.2 / remainingSlots;

        double maxRiskPerTrade = buyingPower * maxRiskPerTradeFraction;
        double maxPositionCost = buyingPower * maxCapitalPerTradeFraction;

        double riskPerShare;

        if (type == "Long")
        {
            if (stopLossPrice >= entryPrice)
            {
                Console.WriteLine("[WARNING]: Stop-loss must be below entry price for long positions.");
                return 0;
            }

            riskPerShare = entryPrice - stopLossPrice;
            if (riskPerShare <= 0)
                return 0;

            int qtyByRisk = (int)Math.Floor(maxRiskPerTrade / riskPerShare);
            int qtyByCapital = (int)Math.Floor(maxPositionCost / entryPrice);

            return Math.Min(qtyByRisk, qtyByCapital);
        }

        if (stopLossPrice <= entryPrice)
        {
            Console.WriteLine("[WARNING]: Stop-loss must be above entry price for short positions.");
            return 0;
        }

        riskPerShare = stopLossPrice - entryPrice;
        if (riskPerShare <= 0)
            return 0;

        int qtyByRiskShort = (int)Math.Floor(maxRiskPerTrade / riskPerShare);
        int qtyByCapitalShort = (int)Math.Floor(maxPositionCost / entryPrice);

        return Math.Min(qtyByRiskShort, qtyByCapitalShort);
    }

    private double CalculateTakeProfit(double p1, double entry, string direction)
    {
        double risk = Math.Abs(entry - p1);
        double rewardMultiplier = 1.2;

        return direction == "Long"
            ? Math.Max(Math.Round(entry + (risk * rewardMultiplier), 2), Math.Round(entry + 0.01, 2))
            : Math.Min(Math.Round(entry - (risk * rewardMultiplier), 2), Math.Round(entry - 0.01, 2));
    }
}