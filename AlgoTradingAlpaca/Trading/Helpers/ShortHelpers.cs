using AlgoTradingAlpaca.Models;

namespace AlgoTradingAlpaca.Trading.Helpers;

public static class ShortHelpers
{
    public static int FindHighPointIndex(List<BarData> barData)
    {
        double highPoint = Double.MinValue;
        int index = -1;
        for (int i = 0; i < barData.Count; i++)
        {
            if (barData[i].ClosingPrice > highPoint)
            {
                highPoint = barData[i].ClosingPrice;
                index = i;
            }
        }
        return index;
    }
    
    public static bool IsValidUpTrend(List<BarData> bars, double highestClose, int highestIndex,
        int lookbackBars, double minimumPercent)
    {
        double lowestPriorPrice = double.MaxValue;
        int startIndex = Math.Max(0, highestIndex - lookbackBars);
        for (int i = startIndex; i < highestIndex; i++)
        {
            if (bars[i].ClosingPrice < lowestPriorPrice)
            {
                lowestPriorPrice = bars[i].ClosingPrice;
            }
        }

        if (lowestPriorPrice <= 0) return false;
        double percentRise = 
            (highestClose - lowestPriorPrice) / lowestPriorPrice;
        return percentRise >= minimumPercent;
    }
    
    public static bool IsValidHighPosition(List<BarData> bars, int highestIndex)
    {
        return highestIndex >= 0 && highestIndex < bars.Count - 2;
    }

    public static List<BarData> BuildBearishTrend(List<BarData> bars, int startIndex)
    {
        int bearishTrendStart = startIndex + 1;
        var bearishTrend = new List<BarData>();
        bearishTrend.Add(bars[startIndex]);
        bearishTrend.Add(bars[bearishTrendStart]);
        double lowestPrice = bars[bearishTrendStart].LowPrice;
        for (int i = bearishTrendStart + 1; i < bars.Count; i++)
        {
            double currentClose = bars[i].ClosingPrice;
            if (currentClose < lowestPrice)
            {
                lowestPrice = currentClose;
                bearishTrend.Add(bars[i]);
            }
            else
            {
                if (bearishTrend.Count > 1)
                    break;
            }
        }
        return bearishTrend;
    }
    
    public static List<BarData> BuildBullishTrend(List<BarData> bars, int bullishTrendStart)
    {
        List<BarData> bullishTrend = new List<BarData>();
        bullishTrend.Add(bars[bullishTrendStart]);
        for (int i = bullishTrendStart + 1; i < bars.Count; i++)
        {
            if (bars[i].ClosingPrice > bars[i - 1].ClosingPrice)
            {
                bullishTrend.Add(bars[i]);
            }
            else
            {
                break;
            }
        }
        return bullishTrend;
    }
}