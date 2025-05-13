using AlgoTradingAlpaca.Models;

namespace AlgoTradingAlpaca.Trading.Helpers;

public static class LongHelpers
{
    public static int FindLowPointIndex(List<BarData> barData)
    {
        double lowestPoint = Double.MaxValue;
        int index = -1;
        for (int i = 0; i < barData.Count; i++)
        {
            if (barData[i].ClosingPrice < lowestPoint)
            {
                lowestPoint = barData[i].LowPrice;
                index = i;
            }
        }
        return index;
    }
    
    public static List<BarData> BuildBullishTrend(List<BarData> bars, int startIndex)
    {
        int bullishTrendStart = startIndex + 1;
        List<BarData> bullishTrend = new List<BarData>();
        bullishTrend.Add(bars[startIndex]);
        bullishTrend.Add(bars[bullishTrendStart]);
        double highestPrice = bars[bullishTrendStart].ClosingPrice;
        for (int i = bullishTrendStart + 1; i < bars.Count; i++)
        {
            if (i >= bars.Count) break;
            if (bars[i].ClosingPrice > highestPrice)
            {
                highestPrice = bars[i].ClosingPrice;
                bullishTrend.Add(bars[i]);
            }
            else
            {
                if (bullishTrend.Count > 1) break;
            }
        }
        return bullishTrend;
    }
    
    public static bool IsValidDownTrend(List<BarData> bars, double lowestClose, int lowestIndex,
        int lookbackBars, double minimumPercent)
    {
        int startIndex = Math.Max(0, lowestIndex - lookbackBars);
        double highestPriorPrice = 0;
        for (int i = startIndex; i < lowestIndex; i++)
        {
            if (bars[i].ClosingPrice > highestPriorPrice)
            {
                highestPriorPrice = bars[i].ClosingPrice;
            }
        }

        if (highestPriorPrice <= 0) 
            return false;
        double percentDecline = 
            (highestPriorPrice - lowestClose) / highestPriorPrice;
        return percentDecline >= minimumPercent;
    } 
    
    public static bool IsValidLowPosition(List<BarData> bars, int lowestIndex)
    {
        return lowestIndex >= 0 && lowestIndex < bars.Count - 2;
    }
    
    public static  List<BarData> BuildBearishTrend(List<BarData> bars, int bearishTrendStart)
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