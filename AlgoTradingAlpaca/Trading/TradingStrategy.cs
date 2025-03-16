using AlgoTradingAlpaca.Data;
using AlgoTradingAlpaca.Interfaces;
using AlgoTradingAlpaca.Models;

namespace AlgoTradingAlpaca.Trading;

public class TradingStrategy
{
    private readonly ITradingClientService _tradingClientService;
    private readonly IBarDataService _barDataService;
    private readonly IPositionDataService _positionDataService;
    private readonly AppDbContext _dbContext;

    public TradingStrategy(ITradingClientService tradingClientService, IBarDataService barDataService, IPositionDataService positionDataService, AppDbContext dbContext)
    {
        _tradingClientService = tradingClientService;
        _barDataService = barDataService;
        _positionDataService = positionDataService;
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
            
            if(!hasOpenPositions)
            {
                await CalculateBullishReversalSignal(symbol, bars);
            }
        }
    }

    private async Task CalculateBullishReversalSignal(string symbol, List<BarData> barData)
    {
        if (barData.Count < 30)
        {
            return;
        }
        
        var orderedBars = barData.OrderBy(b => b.TimeStamp).ToList();
        var orderedAndRecentBars = orderedBars.Skip(Math.Max(0, orderedBars.Count - 30)).ToList();

        var lowestIndex = await FindLowPointIndex(orderedAndRecentBars);
        var lowestClosingPrice = orderedAndRecentBars[lowestIndex].ClosingPrice;

        var validDownTrend = await IsValidDownTrend(orderedAndRecentBars, lowestClosingPrice, lowestIndex,
            lookbackBars: 5, minimumPercent: 0.005);

        if (!validDownTrend)
        {
            return;
        }

        if (!await IsValidLowPosition(orderedAndRecentBars, lowestIndex))
        {
            return;
        }
        
        var bullishTrend = await BuildBullishTrend(orderedAndRecentBars, lowestIndex);
        if (bullishTrend.Count == 0)
        {
            return;
        }
        var bullishTrendLength = bullishTrend.Count;
        var bullishHigh = bullishTrend[bullishTrendLength - 1].ClosingPrice;

        if (bullishHigh < 0)
        {
            return;
        }
        
        var bullishHighIndex = lowestIndex + (bullishTrendLength -1);

        if (bullishHighIndex >= orderedAndRecentBars.Count)
        {
            return;
        }

        var bearishTrendStart = bullishHighIndex + 1;
        if (bearishTrendStart >= orderedAndRecentBars.Count)
        {
            return;
        }
        
        var bearishTrend = await BuildBearishTrend(orderedAndRecentBars, bearishTrendStart);

        if (bearishTrend.Count < 3)
        {
            return;
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
            return;
        }
        
        
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
    
    
}