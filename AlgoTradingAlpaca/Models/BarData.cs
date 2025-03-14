namespace AlgoTradingAlpaca.Models;

public class BarData
{
    public int Id { get; set; }
    public string Symbol { get; set; }
    public double OpenPrice { get; set; }
    public double HighPrice { get; set; }
    public double LowPrice { get; set; }
    public double ClosingPrice { get; set; }
    public double Volume { get; set; }
    public DateTime TimeStamp { get; set; }
}