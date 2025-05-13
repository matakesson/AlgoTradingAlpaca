namespace AlgoTradingAlpaca.Models;

public class TradeSignalResult
{
    public bool IsBreakout { get; init; }
    public double EntryPrice { get; init; }
    public double Point1 { get; init; }
    public double Point2 { get; init; }
    public int Quantity { get; init; }
}