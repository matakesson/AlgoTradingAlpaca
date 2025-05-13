using AlgoTradingAlpaca.Models;

namespace AlgoTradingAlpaca.tests.Mock;

public static class BullishReversalBarsMockSetup
{
    public static List<BarData> GenerateValidBullishSignalData(string symbol)
    {
        var now = DateTime.Now;
        var bars = new List<BarData>();

        for (int i = 0; i < 20; i++)
        {
            double price = 100.0 - i * 0.5;
            bars.Add(new BarData
            {
                Symbol = symbol,
                TimeStamp = now.AddMinutes(i),
                LowPrice = price - 0.1,
                ClosingPrice = price
            });
        }

        bars.Add(new BarData
        {
            Symbol = symbol,
            TimeStamp = now.AddMinutes(20),
            LowPrice = 89.0,
            ClosingPrice = 89.0
        });

        double[] bullishPrices = { 90.0, 92.0, 94.0, 96.0 };
        for (int i = 0; i < 4; i++)
        {
            bars.Add(new BarData
            {
                Symbol = symbol,
                TimeStamp = now.AddMinutes(21 + i),
                LowPrice = bullishPrices[i] - 0.1,
                ClosingPrice = bullishPrices[i]
            });
        }

        double[] bearishPrices = { 95.0, 94.0, 93.0 };
        for (int i = 0; i < 3; i++)
        {
            bars.Add(new BarData
            {
                Symbol = symbol,
                TimeStamp = now.AddMinutes(25 + i),
                LowPrice = bearishPrices[i] - 0.1,
                ClosingPrice = bearishPrices[i]
            });
        }

        bars.Add(new BarData
        {
            Symbol = symbol,
            TimeStamp = now.AddMinutes(28),
            LowPrice = 96.5,
            ClosingPrice = 97.0
        });

        bars.Add(new BarData
        {
            Symbol = symbol,
            TimeStamp = now.AddMinutes(29),
            LowPrice = 97.5,
            ClosingPrice = 98.0
        });

        Console.WriteLine($"Generated test data with {bars.Count} bars");
        Console.WriteLine($"Bullish high: 96.0");
        Console.WriteLine($"Final price: {bars.Last().ClosingPrice}");

        return bars;
    }

    public static List<BarData> GenerateFlatMarketData(string symbol)
    {
        var now = DateTime.Now;
        var bars = new List<BarData>();

        for (int i = 0; i < 30; i++)
        {
            double price = 100.0 + (new Random(i).NextDouble() * 4 - 2);
            bars.Add(new BarData
            {
                Symbol = symbol,
                TimeStamp = now.AddMinutes(i),
                LowPrice = price - 0.1,
                ClosingPrice = price
            });
        }

        return bars;
    }

    public static List<BarData> GenerateOnlyDowntrendData(string symbol)
    {
        var now = DateTime.Now;
        var bars = new List<BarData>();

        for (int i = 0; i < 30; i++)
        {
            double price = 100.0 - i * 0.5;
            bars.Add(new BarData
            {
                Symbol = symbol,
                TimeStamp = now.AddMinutes(i),
                LowPrice = price - 0.1,
                ClosingPrice = price
            });
        }

        return bars;
    }

    public static List<BarData> GenerateShortBearishTrendData(string symbol)
    {
        var now = DateTime.Now;
        var bars = new List<BarData>();

        for (int i = 0; i < 20; i++)
        {
            double price = 100.0 - i * 0.5;
            bars.Add(new BarData
            {
                Symbol = symbol,
                TimeStamp = now.AddMinutes(i),
                LowPrice = price - 0.1,
                ClosingPrice = price
            });
        }

        bars.Add(new BarData
        {
            Symbol = symbol,
            TimeStamp = now.AddMinutes(20),
            LowPrice = 85.0,
            ClosingPrice = 85.0
        });

        double[] bullishPrices = { 86.0, 90.0, 93.0, 96.0 };
        for (int i = 0; i < 4; i++)
        {
            bars.Add(new BarData
            {
                Symbol = symbol,
                TimeStamp = now.AddMinutes(21 + i),
                LowPrice = bullishPrices[i] - 0.1,
                ClosingPrice = bullishPrices[i]
            });
        }

        double[] bearishPrices = { 95.0, 94.0 };
        for (int i = 0; i < 2; i++)
        {
            bars.Add(new BarData
            {
                Symbol = symbol,
                TimeStamp = now.AddMinutes(25 + i),
                LowPrice = bearishPrices[i] - 0.1,
                ClosingPrice = bearishPrices[i]
            });
        }

        bars.Add(new BarData
        {
            Symbol = symbol,
            TimeStamp = now.AddMinutes(27),
            LowPrice = 96.5,
            ClosingPrice = 97.0
        });

        bars.Add(new BarData
        {
            Symbol = symbol,
            TimeStamp = now.AddMinutes(28),
            LowPrice = 97.5,
            ClosingPrice = 98.0
        });

        bars.Add(new BarData
        {
            Symbol = symbol,
            TimeStamp = now.AddMinutes(29),
            LowPrice = 98.0,
            ClosingPrice = 98.5
        });

        return bars;
    }

    public static List<BarData> GenerateNoBreakoutData(string symbol)
    {
        var now = DateTime.Now;
        var bars = new List<BarData>();

        for (int i = 0; i < 20; i++)
        {
            double price = 100.0 - i * 0.5;
            bars.Add(new BarData
            {
                Symbol = symbol,
                TimeStamp = now.AddMinutes(i),
                LowPrice = price - 0.1,
                ClosingPrice = price
            });
        }

        bars.Add(new BarData
        {
            Symbol = symbol,
            TimeStamp = now.AddMinutes(20),
            LowPrice = 85.0,
            ClosingPrice = 85.0
        });

        double[] bullishPrices = { 86.0, 90.0, 93.0, 96.0 };
        for (int i = 0; i < 4; i++)
        {
            bars.Add(new BarData
            {
                Symbol = symbol,
                TimeStamp = now.AddMinutes(21 + i),
                LowPrice = bullishPrices[i] - 0.1,
                ClosingPrice = bullishPrices[i]
            });
        }

        double[] bearishPrices = { 95.0, 94.0, 93.0 };
        for (int i = 0; i < 3; i++)
        {
            bars.Add(new BarData
            {
                Symbol = symbol,
                TimeStamp = now.AddMinutes(25 + i),
                LowPrice = bearishPrices[i] - 0.1,
                ClosingPrice = bearishPrices[i]
            });
        }

        bars.Add(new BarData
        {
            Symbol = symbol,
            TimeStamp = now.AddMinutes(28),
            LowPrice = 94.5,
            ClosingPrice = 95.0 
        });

        bars.Add(new BarData
        {
            Symbol = symbol,
            TimeStamp = now.AddMinutes(29),
            LowPrice = 95.5,
            ClosingPrice = 95.8 
        });

        return bars;
    }

    public static List<BarData> GenerateStrongBreakoutData(string symbol)
    {
        var now = DateTime.Now;
        var bars = new List<BarData>();

        for (int i = 0; i < 20; i++)
        {
            double price = 100.0 - i * 0.5;
            bars.Add(new BarData
            {
                Symbol = symbol,
                TimeStamp = now.AddMinutes(i),
                LowPrice = price - 0.1,
                ClosingPrice = price
            });
        }

        bars.Add(new BarData
        {
            Symbol = symbol,
            TimeStamp = now.AddMinutes(20),
            LowPrice = 85.0,
            ClosingPrice = 85.0
        });

        double[] bullishPrices = { 86.0, 90.0, 93.0, 96.0 };
        for (int i = 0; i < 4; i++)
        {
            bars.Add(new BarData
            {
                Symbol = symbol,
                TimeStamp = now.AddMinutes(21 + i),
                LowPrice = bullishPrices[i] - 0.1,
                ClosingPrice = bullishPrices[i]
            });
        }

        double[] bearishPrices = { 95.0, 94.0, 93.0 };
        for (int i = 0; i < 3; i++)
        {
            bars.Add(new BarData
            {
                Symbol = symbol,
                TimeStamp = now.AddMinutes(25 + i),
                LowPrice = bearishPrices[i] - 0.1,
                ClosingPrice = bearishPrices[i]
            });
        }
        bars.Add(new BarData
        {
            Symbol = symbol,
            TimeStamp = now.AddMinutes(28),
            LowPrice = 98.0,
            ClosingPrice = 99.0
        });

        bars.Add(new BarData
        {
            Symbol = symbol,
            TimeStamp = now.AddMinutes(29),
            LowPrice = 99.0,
            ClosingPrice = 100.0 
        });
        return bars;
    }

}

