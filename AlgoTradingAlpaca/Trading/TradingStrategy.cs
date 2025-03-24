using System.Globalization;
using AlgoTradingAlpaca.Data;
using AlgoTradingAlpaca.Interfaces;
using AlgoTradingAlpaca.Models;

namespace AlgoTradingAlpaca.Trading;

public class TradingStrategy : ITradingStrategy
{
    private readonly ITradingClientService _tradingClientService;
    private readonly IBarDataService _barDataService;
    private readonly IPositionDataService _positionDataService;
    private readonly IAccountDataService _accountDataService;
    private readonly AppDbContext _dbContext;

    public TradingStrategy(ITradingClientService tradingClientService, IBarDataService barDataService, IPositionDataService positionDataService, AppDbContext dbContext,
        IAccountDataService accountDataService)
    {
        _tradingClientService = tradingClientService;
        _barDataService = barDataService;
        _positionDataService = positionDataService;
        _accountDataService = accountDataService;
        _dbContext = dbContext;
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
    Console.WriteLine($"[POSITIONS] Found {positions.Count} total positions");

    var threeMinBars = GenerateThreeMinuteBars(groupedAndSortedData);
    
    foreach (var kvp in threeMinBars)
    {
        var symbol = kvp.Key;
        var bars = kvp.Value;
        Console.WriteLine($"[SYMBOL] Processing {symbol} with {bars.Count} bars");

        bool hasOpenPositions = positions.Any(p => p.Symbol == symbol && p.Status == "Open");
        Console.WriteLine($"[POSITION] {symbol} has open positions: {hasOpenPositions}");

        var zigzagPoints = await ZigZagBars(bars);
        Console.WriteLine($"[ZIGZAG] {symbol} found {zigzagPoints.Count} zigzag points");

        if(!hasOpenPositions)
        {
            Console.WriteLine($"[SIGNAL] Checking bullish reversal for {symbol}");
            bool bullishReversalSignal = await CalculateBullishReversalSignal(symbol, bars);

            if (bullishReversalSignal)
            {
                Console.WriteLine($"[SIGNAL] Bullish reversal detected for {symbol}");
                if (zigzagPoints.Count >= 2)
                {
                    var latestZigzagPoint = zigzagPoints.Last();
                    var previousZigzagPoint = zigzagPoints[^2];
                    double currentPrice = bars.Last().ClosingPrice;
                    
                    Console.WriteLine($"[ZIGZAG] Last 2 points: {previousZigzagPoint.Type}@{previousZigzagPoint.Price} -> " +
                                     $"{latestZigzagPoint.Type}@{latestZigzagPoint.Price}");
                    
                    if (previousZigzagPoint.Type == "High" && latestZigzagPoint.Type == "Low" && currentPrice > latestZigzagPoint.Price)
                    {
                        Console.WriteLine($"[ENTRY] Valid pattern detected for {symbol}");
                        var calcQuantity = await CalculateOrderQuantity();
                        int quantity = (int)(calcQuantity/ currentPrice);
                        
                        Console.WriteLine($"[ORDER] Calculated quantity: {quantity}");

                        if(quantity > 0)
                        {
                            await _tradingClientService.PlaceMarketOrderAsync(symbol, quantity, "buy"); 
                            Console.WriteLine($"📈 [EXECUTION] Long Entry: {quantity} shares bought at {currentPrice}");
                            
                            _dbContext.Add(new Position
                            {
                                Symbol = symbol,
                                Qty = quantity,
                                AverageEntryPrice = currentPrice,
                                ClosingPrice = currentPrice,
                                OpenTime = DateTime.Now,
                                Status = "Open",
                                Type = "Long"
                            });
                            await _dbContext.SaveChangesAsync();
                            Console.WriteLine($"[DATABASE] New position saved for {symbol}");
                        }
                        else
                        {
                            Console.WriteLine($"[ERROR] Invalid quantity for {symbol}: {quantity}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[ENTRY] Pattern mismatch for {symbol}");
                    }
                }
                else
                {
                    Console.WriteLine($"[ENTRY] Insufficient zigzag points ({zigzagPoints.Count}) for {symbol}");
                }
            }
            else
            {
                Console.WriteLine($"[SIGNAL] No bullish reversal for {symbol}");
            }
        }
        else
        {
            // Existing position management code
            if (zigzagPoints.Any())
            {
                var latestZigzag = zigzagPoints.Last();
                Console.WriteLine($"[EXIT] Checking exit conditions for {symbol}");
                double currentPrice = bars.Last().ClosingPrice;
                var position = _dbContext.Positions.FirstOrDefault(s => s.Symbol == symbol && s.Status == "Open");

                if(position == null)
                {
                    Console.WriteLine($"[ERROR] No open position found for {symbol}");
                    continue;
                }

                if (position.Type == "Long" && latestZigzag.Type.Equals("low", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"[EXIT] Long exit signal detected for {symbol}");
                    await _tradingClientService.CloseOrderAsync(symbol, 100);
                    
                    var existingPosition = _dbContext.Positions.FirstOrDefault(s => s.Symbol == symbol && s.Status == "Open");
                    if (existingPosition != null)
                    {
                        existingPosition.Status = "Closed";
                        await _dbContext.SaveChangesAsync();
                        Console.WriteLine($"[EXIT] Closed long position for {symbol}");
                    }
                }

                if (position.Type == "Short" && latestZigzag.Type.Equals("high", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"[EXIT] Short exit signal detected for {symbol}");
                    await _tradingClientService.CloseOrderAsync(symbol, 100);
                    
                    var existingPosition = _dbContext.Positions.FirstOrDefault(s => s.Symbol == symbol && s.Status == "Open");
                    if (existingPosition != null)
                    {
                        existingPosition.Status = "Closed";
                        await _dbContext.SaveChangesAsync();
                        Console.WriteLine($"[EXIT] Closed short position for {symbol}");
                    }
                }
            }
        }
    }
    Console.WriteLine("[STRATEGY] Execution completed");
}



    private async Task<bool> CalculateBullishReversalSignal(string symbol, List<BarData> barData)
    {
        if (barData.Count < 30)
        {
            Console.WriteLine($"Not enough data for {symbol}");
            return false;
        }
        
        var orderedBars = barData.OrderBy(b => b.TimeStamp).ToList();
        var orderedAndRecentBars = orderedBars.Skip(Math.Max(0, orderedBars.Count - 30)).ToList();

        var lowestIndex = await FindLowPointIndex(orderedAndRecentBars);
        var lowestClosingPrice = orderedAndRecentBars[lowestIndex].ClosingPrice;

        var validDownTrend = await IsValidDownTrend(orderedAndRecentBars, lowestClosingPrice, lowestIndex,
            lookbackBars: 5, minimumPercent: 0.003);

        if (!validDownTrend)
        {
            return false;
        }

        if (!await IsValidLowPosition(orderedAndRecentBars, lowestIndex))
        {
            return false;
        }
        
        var bullishTrend = await BuildBullishTrend(orderedAndRecentBars, lowestIndex);
        if (bullishTrend.Count == 0)
        {
            return false;
        }
        var bullishTrendLength = bullishTrend.Count;
        var bullishHigh = bullishTrend[bullishTrendLength - 1].ClosingPrice;

        if (bullishHigh < 0)
        {
            return false;
        }
        
        var bullishHighIndex = lowestIndex + (bullishTrendLength -1);

        if (bullishHighIndex >= orderedAndRecentBars.Count)
        {
            return false;
        }

        var bearishTrendStart = bullishHighIndex + 1;
        if (bearishTrendStart >= orderedAndRecentBars.Count)
        {
            return false;
        }
        
        var bearishTrend = await BuildBearishTrend(orderedAndRecentBars, bearishTrendStart);
        double bearishTrendLow = bearishTrend.Last().ClosingPrice;

        if (bearishTrend.Count < 3)
        {
            return false;
        }
        
        int bearishTrendIndex = bearishTrendStart + bearishTrend.Count - 1;
        bearishTrendIndex = Math.Min(bearishTrendIndex, orderedAndRecentBars.Count - 1);

        bool breakoutDetected = false;
        double currentPrice = orderedAndRecentBars.Last().ClosingPrice;
        double priceOneBarsAgo = orderedAndRecentBars[orderedAndRecentBars.Count - 1].ClosingPrice;
        if (currentPrice > bullishHigh)
        {
            breakoutDetected = true;
        }

        if (!breakoutDetected)
        {
            return false;
        }

        return true;

        // int quantity = await CalculateOrderQuantity();
        //
        // if (quantity <= 0)
        // {
        //     Console.WriteLine($"ERROR: {symbol}: Invalid quantity {quantity}");
        //     return;
        // }
        //
        // double takeProfit = await CalculateTakeProfit(lowestClosingPrice, bullishHigh, bearishTrendLow);
        // double stopLoss = Math.Round(lowestClosingPrice, 2);
        //
        // await _tradingClientService.PlaceMarketOrderAsync(symbol, quantity, "buy", currentPrice, takeProfit, stopLoss );
        //
        // _dbContext.Add(new Position
        // {
        //     Symbol = symbol,
        //     Qty = quantity,
        //     AverageEntryPrice = currentPrice,
        //     ClosingPrice = currentPrice,
        //     OpenTime = DateTime.Now,
        //     TakeProfit = takeProfit,
        //     StopLoss = stopLoss,
        //     Status = "Open",
        //     Type = "Long"
        // });
        // await _dbContext.SaveChangesAsync();

    }

    private async Task<int> FindLowPointIndex(List<BarData> barData)
    {
        double lowestPoint = Double.MaxValue;
        int index = -1;
        for (int i = 0; i < barData.Count; i++)
        {
            if (barData[i].ClosingPrice < lowestPoint)
            {
                lowestPoint = barData[i].ClosingPrice;
                index = i;
            }
        }
        return index;
    }

    private async Task<List<BarData>> BuildBullishTrend(List<BarData> bars, int startIndex)
    {
        int bullishTrendStart = startIndex + 1;
        List<BarData> bullishTrend = new List<BarData>();
        
        bullishTrend.Add(bars[startIndex]);
        bullishTrend.Add(bars[bullishTrendStart]);
        
        double highestPrice = bars[bullishTrendStart].ClosingPrice;
        int highestPriceIndex = bullishTrendStart;

        for (int i = bullishTrendStart + 1; i < bars.Count; i++)
        {
            if (i >= bars.Count) break;

            if (bars[i].ClosingPrice > highestPrice)
            {
                highestPrice = bars[i].ClosingPrice;
                highestPriceIndex = i;
                
                bullishTrend.Add(bars[i]);
            }
            else
            {
                if (bullishTrend.Count > 1) break;
            }
        }
        return bullishTrend;
    }

    private async Task<bool> IsValidDownTrend(List<BarData> bars, double lowestClose, int lowestIndex,
        int lookbackBars, double minimumPercent)
    {
        if (lowestIndex < lookbackBars)
        {
            return false;
        }

        double highestPriorPrice = 0;
        int startIndex = lowestIndex - lookbackBars;
        
        for (int i = startIndex; i < lowestIndex; i++)
        {
            if (bars[i].ClosingPrice > highestPriorPrice)
            {
                highestPriorPrice = bars[i].ClosingPrice;
            }
        }

        if (highestPriorPrice <= 0) return false;

        double priceDrop = highestPriorPrice - lowestClose;
        double percentDecline = priceDrop / highestPriorPrice;
        
        return percentDecline >= minimumPercent;
    }

    private async Task<bool> IsValidLowPosition(List<BarData> bars, int lowestIndex)
    {
        return lowestIndex >= 0 && lowestIndex < bars.Count - 2;
    }

    private async Task<List<BarData>> BuildBearishTrend(List<BarData> bars, int bearishTrendStart)
    {
        List<BarData> bearishTrend = new List<BarData>();
        
        bearishTrend.Add(bars[bearishTrendStart]);
        
        for (int i = bearishTrendStart + 1; i < bars.Count; i++)
        {
            if (bars[i].ClosingPrice < bars[i - 1].ClosingPrice)
            {
                bearishTrend.Add(bars[i]);
            }
            else
            {
                break;
            }
        }
        
        return bearishTrend;
    }
    
    private async Task<int> CalculateOrderQuantity()
    {
        var account = await _accountDataService.GetAccountAsync();

        var openPositions = _dbContext.Positions.Where(p => p.Status == "Open").Count();
        
        double accountBalance = Convert.ToDouble(account.buying_power, CultureInfo.InvariantCulture);

        double cashToInvest;
        double totalBuyingPowerPerPosition = accountBalance / 5;

        if (openPositions >= 5)
        {
            return 0;
        }
        else
        {
            cashToInvest = totalBuyingPowerPerPosition;
        }
        return (int)totalBuyingPowerPerPosition;
    }

    private async Task<double> CalculateTakeProfit(double p1, double p2, double p3)
    {
        double midpointY = (p1 + p3) / 2;
        double d = Math.Abs(p2 - midpointY);

        double fibTarget = p2 + (p2 - p1) * 1.618;

        return Math.Round(fibTarget, 2);
    }

    private async Task<List<ZigZagPoint>> ZigZagBars(List<BarData> bars)
{
    double percentage = 0.2;
    var zigZag = new List<ZigZagPoint>();
    
    if (bars.Count <= 30) return zigZag;
    
    double currentHigh = bars[0].HighPrice;
    double currentLow = bars[0].LowPrice;
    int lastDirection = 0; 
    
    for (int i = 1; i < Math.Min(5, bars.Count); i++)
    {
        if (bars[i].HighPrice > currentHigh)
            currentHigh = bars[i].HighPrice;
        if (bars[i].LowPrice < currentLow)
            currentLow = bars[i].LowPrice;
    }
    
    double highLowRatio = (bars[0].HighPrice / currentHigh);
    double lowHighRatio = (bars[0].LowPrice / currentLow);
    
    if (highLowRatio < lowHighRatio) 
    {
        zigZag.Add(new ZigZagPoint
        {
            Index = 0,
            Price = bars[0].HighPrice,
            Type = "High"
        });
        lastDirection = -1; 
    }
    else 
    {
        zigZag.Add(new ZigZagPoint
        {
            Index = 0,
            Price = bars[0].LowPrice,
            Type = "Low"
        });
        lastDirection = 1; 
    }
    
    double swingHigh = bars[0].HighPrice;
    double swingLow = bars[0].LowPrice;
    int swingHighIndex = 0;
    int swingLowIndex = 0;
    
    for (int i = 1; i < bars.Count; i++)
    {
        if (bars[i].HighPrice > swingHigh)
        {
            swingHigh = bars[i].HighPrice;
            swingHighIndex = i;
        }
        
        if (bars[i].LowPrice < swingLow)
        {
            swingLow = bars[i].LowPrice;
            swingLowIndex = i;
        }
        
        if (lastDirection == -1) 
        {
            double lastHighPrice = zigZag.Last().Price;
            
            if (swingLow < lastHighPrice * (1 - percentage / 100))
            {
                zigZag.Add(new ZigZagPoint
                {
                    Index = swingLowIndex,
                    Price = swingLow,
                    Type = "Low"
                });
                
                swingHigh = bars[i].HighPrice;
                swingHighIndex = i;
                
                lastDirection = 1;
            }
        }
        else if (lastDirection == 1) 
        {
            double lastLowPrice = zigZag.Last().Price;
            
            if (swingHigh > lastLowPrice * (1 + percentage / 100))
            {
                zigZag.Add(new ZigZagPoint
                {
                    Index = swingHighIndex,
                    Price = swingHigh,
                    Type = "High"
                });
                
                swingLow = bars[i].LowPrice;
                swingLowIndex = i;
                
                lastDirection = -1;
            }
        }
    }
    
    for (int i = 1; i < zigZag.Count; i++)
    {
        if (zigZag[i].Type == zigZag[i-1].Type)
        {
            // If we have two consecutive points of the same type, remove the one with the less extreme value
            if (zigZag[i].Type == "High")
            {
                if (zigZag[i].Price > zigZag[i-1].Price)
                    zigZag.RemoveAt(i-1);
                else
                    zigZag.RemoveAt(i);
                i--; 
            }
            else 
            {
                if (zigZag[i].Price < zigZag[i-1].Price)
                    zigZag.RemoveAt(i-1);
                else
                    zigZag.RemoveAt(i);
            }
        }
    }
    
    Console.WriteLine($"[ZIGZAG] Generated {zigZag.Count} points with alternating High/Low pattern");
    return zigZag;
}

    private Dictionary<string, List<BarData>> GenerateThreeMinuteBars(Dictionary<string, List<BarData>> groupedData)
    {
        var threeMinuteBars = new Dictionary<string, List<BarData>>();

        foreach (var symbol in groupedData.Keys)
        {
            var bars = groupedData[symbol];
            var threeMinBarsForSymbol = new List<BarData>();
        
            
            if (bars.Count < 3)
            {
                Console.WriteLine($"[3MIN] Skipping {symbol} - insufficient bars ({bars.Count})");
                continue;
            }

            for (int i = 0; i < bars.Count; i += 3)
            {
                var subset = bars.Skip(i).Take(3).ToList();
                if (subset.Count < 3) break;

                var threeMinBar = new BarData
                {
                    Symbol = symbol,
                    OpenPrice = subset.First().OpenPrice,
                    HighPrice = subset.Max(b => b.HighPrice),
                    LowPrice = subset.Min(b => b.LowPrice),
                    ClosingPrice = subset.Last().ClosingPrice,
                    TimeStamp = subset.Last().TimeStamp,
                    Volume = subset.Sum(b => b.Volume)
                };
                threeMinBarsForSymbol.Add(threeMinBar);
            }

            if (threeMinBarsForSymbol.Count > 0)
            {
                threeMinuteBars[symbol] = threeMinBarsForSymbol;
            }
        }
        return threeMinuteBars;
    }
}