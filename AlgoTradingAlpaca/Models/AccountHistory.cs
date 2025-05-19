namespace AlgoTradingAlpaca.Models;

public class AccountHistory
{
    public double[] timestamp { get; set; }
    public double[] equity { get; set; }
    public double[] profit_loss { get; set; }
    public double[] profit_loss_pct { get; set; }
    public double base_value { get; set; }
    public DateTime base_value_asof { get; set; }
    public string timeframe { get; set; }
}