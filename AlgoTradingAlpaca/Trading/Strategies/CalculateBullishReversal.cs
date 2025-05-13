using AlgoTradingAlpaca.Interfaces;
using AlgoTradingAlpaca.Models;
using AlgoTradingAlpaca.Trading.Helpers;

namespace AlgoTradingAlpaca.Trading.Strategies;

public class CalculateBullishReversal : ICalculateBullishReversal
{
    public async Task<TradeSignalResult?> CalculateBullishReversalSignal(List<BarData> barData)
    {
        const int lookbackPeriod = 30;
        const double minDownTrendPercentage = 0.0005;
        if (barData.Count < lookbackPeriod)
            return null;
        
        var orderedBars = barData.OrderBy(b => b.TimeStamp).ToList();
        var recentBars = orderedBars.Skip(Math.Max(0, orderedBars.Count - lookbackPeriod)).ToList();
        var lowIndex = LongHelpers.FindLowPointIndex(recentBars);
        var lowClose = recentBars[lowIndex].ClosingPrice;
        if (!LongHelpers.IsValidDownTrend(recentBars, lowClose, lowIndex, lookbackPeriod, minDownTrendPercentage))
            return null;
        
        if (!LongHelpers.IsValidLowPosition(recentBars, lowIndex))
            return null;
        var bullishTrend = LongHelpers.BuildBullishTrend(recentBars, lowIndex);
        if (bullishTrend.Count == 0)
            return null;
        var bullishHighIndex = lowIndex + bullishTrend.Count - 1;
        if (bullishHighIndex >= recentBars.Count)
            return null;
        
        double bullishHigh = bullishTrend.Last().ClosingPrice;
        var bearishStart = bullishHighIndex + 1;
        if (bearishStart >= recentBars.Count)
            return null;
        var bearishTrend = LongHelpers.BuildBearishTrend(recentBars, bearishStart);
        if (bearishTrend.Count < 3)
            return null;
        
        double currentPrice = recentBars.Last().ClosingPrice;
        double prevPrice = recentBars[recentBars.Count - 2].ClosingPrice;
        bool isBreakout = currentPrice > bullishHigh;
        Console.WriteLine(currentPrice);
        if (!isBreakout)
            return null;
        
        return new TradeSignalResult
        {
            IsBreakout = true,
            EntryPrice = currentPrice,
            Point1 = lowClose,
            Point2 = bullishHigh
        };
    }
}