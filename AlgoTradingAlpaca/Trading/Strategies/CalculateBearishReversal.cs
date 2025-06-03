using AlgoTradingAlpaca.Interfaces;
using AlgoTradingAlpaca.Models;
using AlgoTradingAlpaca.Trading.Helpers;

namespace AlgoTradingAlpaca.Trading.Strategies;

public class CalculateBearishReversal : ICalculateBearishReversal
{
    public async Task<TradeSignalResult?> CalculateBearishReversalSignal(List<BarData> barData)
    {
        const int lookbackPeriod = 30;
        const double minUpTrendPercentage = 0.0005;
        if (barData.Count < lookbackPeriod)
            return null;
        var orderedBars = barData.OrderBy(b => b.TimeStamp).ToList();
        var recentBars = orderedBars.Skip(Math.Max(0, orderedBars.Count - lookbackPeriod)).ToList();
        var highIndex = ShortHelpers.FindHighPointIndex(recentBars);
        if (highIndex < 0 || highIndex >= recentBars.Count)
            return null;
        var highClose = recentBars[highIndex].HighPrice;
        if (!ShortHelpers.IsValidUpTrend(recentBars, highClose, highIndex, lookbackPeriod, minUpTrendPercentage)) 
            return null;
        if (!ShortHelpers.IsValidHighPosition(recentBars, highIndex))
            return null;
        var bearishTrend = ShortHelpers.BuildBearishTrend(recentBars, highIndex);
        if (bearishTrend.Count == 0)
            return null;
        var bearishLowIndex = highIndex + bearishTrend.Count - 1;
        if (bearishLowIndex >= recentBars.Count)
            return null;
        double bearishLow = bearishTrend.Last().LowPrice;
        var bullishStart = bearishLowIndex + 1;
        if (bullishStart >= recentBars.Count)
            return null;
        var bullishTrend = ShortHelpers.BuildBullishTrend(recentBars, bullishStart);
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

    
}