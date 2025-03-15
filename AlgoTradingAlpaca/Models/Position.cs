namespace AlgoTradingAlpaca.Models;

public class Position
{
    public int Id { get; set; }
    public string Symbol { get; set; }
    public int Qty { get; set; }
    public double AverageEntryPrice { get; set; }
    public DateTime OpenTime { get; set; }
    public double ClosingPrice { get; set; }
    public DateTime ExitTime { get; set; } // For time-based exits
    public double StopLoss { get; set; } // Stop loss price level
    public double TakeProfit { get; set; } // Take profit price level
    public string Status { get; set; }
    public string Type { get; set; }
}