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
        var barData = await _barDataService.GetBarDataAsync();
        
        
        var groupedAndSortedData = barData
            .GroupBy(bar => bar.Symbol)
            .ToDictionary(
                group => group.Key,
                group => group.OrderBy(bar => bar.TimeStamp).ToList()
            );

        var positions = await _positionDataService.GetPositionsAsync();

        
        foreach (var kvp in groupedAndSortedData)
        {
            var symbol = kvp.Key;
            var bars = kvp.Value;

            bool hasOpenPositions = positions
                .Any(p => p.Symbol == symbol && p.Status == "Open");
            
            var zigzagPoints = await ZigZagBars(bars);
            
            if(!hasOpenPositions)
            {
                bool bullishReversalSignal = await CalculateBullishReversalSignal(symbol, bars);

                if (bullishReversalSignal)
                {
                    if (zigzagPoints.Count >= 2)
                    {
                        var latestZigzagPoint = zigzagPoints.Last();
                        var previousZigzagPoint = zigzagPoints[^2];
                        double currentPrice = bars.Last().ClosingPrice;
                        
                        if (previousZigzagPoint.Type == "High" && latestZigzagPoint.Type == "Low" && currentPrice > latestZigzagPoint.Price)
                        {
                            var quantity = await CalculateOrderQuantity();
                            await _tradingClientService.PlaceMarketOrderAsync(symbol, quantity, "buy"); 
                            Console.WriteLine($"📈 Long Entry: Bought at {currentPrice}");
                            
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
                        }
                    }
                }
            }
            
            else if (hasOpenPositions)
            {
                if (zigzagPoints.Any())
                {
                    var latestZigzag =  zigzagPoints.Last();
                    double currentPrice = bars.Last().ClosingPrice;
                    var position = _dbContext.Positions.Where(s => s.Symbol == symbol && s.Status == "Open").FirstOrDefault();

                    if (position.Type == "Long" && latestZigzag.Type == "low")
                    {
                        await _tradingClientService.CloseOrderAsync(symbol, 100);
                        
                        var existingPosition = _dbContext.Positions.Where(s => s.Symbol == symbol && s.Status == "Open").FirstOrDefault();
                        if (existingPosition != null)
                        {
                            existingPosition.Status = "Closed";
                            _dbContext.SaveChanges();
                        }
                    }

                    if (position.Type == "Short" && latestZigzag.Type == "high")
                    {
                        await _tradingClientService.CloseOrderAsync(symbol, 100);
                        
                        var existingPosition = _dbContext.Positions.Where(s => s.Symbol == symbol && s.Status == "Open").FirstOrDefault();
                        if (existingPosition != null)
                        {
                            existingPosition.Status = "Closed";
                            _dbContext.SaveChanges();
                        }
                    }
                }
            }
        }
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
            lookbackBars: 5, minimumPercent: 0.005);

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
        if (currentPrice > bullishHigh && priceOneBarsAgo < bullishHigh)
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
        double lowestPoint = 0;
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
        var ZigZag = new List<ZigZagPoint>();
        var knasturn = bars[bars.Count - 1].ClosingPrice;
        int direction = 0;

        if (bars.Count > 60)
        {
            for (int i = 0; i < bars.Count; i++)
            {
                var price =  bars[i].ClosingPrice;
                var highest = bars[i].HighPrice;
                var lowest = bars[i].LowPrice;

                if (direction == 0)
                {
                    if (price > knasturn * (1 + percentage / 100))
                    {
                        direction = 1;
                        ZigZag.Add(new ZigZagPoint
                        {
                            Index = i,
                            Price = lowest,
                            Type = "Low"
                        });
                    }
                    else if (price < knasturn * (1 - percentage / 100))
                    {
                        direction = -1;
                        ZigZag.Add(new ZigZagPoint
                        {
                            Index = i,
                            Price = highest,
                            Type = "High"
                        });
                    }
                }
                else if (direction == 1)
                {
                    if (highest > knasturn)
                    {
                        knasturn = highest;
                    }
                    else if (lowest < knasturn * (1 - percentage / 100))
                    {
                        ZigZag.Add(new ZigZagPoint
                        {
                            Index = i,
                            Price = knasturn,
                            Type = "High"
                        });
                        knasturn = lowest;
                        direction = -1;
                    }
                }
                else if (direction == -1)
                {
                    if (lowest < knasturn)
                    {
                        knasturn = lowest;
                    }
                    else if (highest > knasturn * (1 + percentage / 100))
                    {
                        ZigZag.Add(new ZigZagPoint
                        {
                            Index = i,
                            Price = knasturn,
                            Type = "Low"
                        });
                        knasturn = highest;
                        direction = 1;
                    }
                }
            }
        }
        return ZigZag;

    }
    
    
}