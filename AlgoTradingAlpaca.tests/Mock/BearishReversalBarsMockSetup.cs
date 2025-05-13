using AlgoTradingAlpaca.Models;

namespace AlgoTradingAlpaca.tests.Mock;

public static class BearishReversalBarsMockSetup
{
    public static List<BarData> GenerateValidBearishSignalData(string symbol)
    {
        var now = DateTime.Now;
        var bars = new List<BarData>();

        for (int i = 0; i < 22; i++)
        {
            double price = 50.0 + i * 0.5;
            bars.Add(new BarData
            {
                Symbol = symbol,
                TimeStamp = now.AddMinutes(i),
                LowPrice = price - 0.1,
                HighPrice = price + 0.2,
                ClosingPrice = price
            });
        }
        
        bars.Add(new BarData
        {
            Symbol = symbol,
            TimeStamp = now.AddMinutes(20),
            LowPrice = 60.9,
            HighPrice = 61.0,
            ClosingPrice = 61.0
        });
        
        double[] bearishPrices = { 60.0, 58.0, 56.5, 55.0, 54.0 };
        for (int i = 0; i < bearishPrices.Length; i++)
        {
            double price = bearishPrices[i];
            bars.Add(new BarData
            {
                Symbol = symbol,
                TimeStamp = now.AddMinutes(23 + i),
                LowPrice = price - 0.2,
                HighPrice = price + 0.2,
                ClosingPrice = price
            });
        }
        
        double[] retracePrices = { 54.8, 55.5 };
        for (int i = 0; i < retracePrices.Length; i++)
        {
            double price = retracePrices[i];
            bars.Add(new BarData
            {
                Symbol = symbol,
                TimeStamp = now.AddMinutes(28 + i),
                LowPrice = price - 0.2,
                HighPrice = price + 0.2,
                ClosingPrice = price
            });
        }
        bars.Add(new BarData
        {
            Symbol = symbol,
            TimeStamp = now.AddMinutes(30),
            LowPrice = 56.2,
            HighPrice = 56.8,
            ClosingPrice = 56.2
        });

        bars.Add(new BarData
        {
            Symbol = symbol,
            TimeStamp = now.AddMinutes(30),
            LowPrice = 52.8,
            HighPrice = 53.0,
            ClosingPrice = 52.9
        });

        Console.WriteLine($"Generated valid bearish signal data with {bars.Count} bars");
        return bars;
    }
}