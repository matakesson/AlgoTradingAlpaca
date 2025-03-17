namespace AlgoTradingAlpaca.Models;

public class Account
{
    public string id { get; set; }
    public Admin_Configurations admin_configurations { get; set; }
    public object user_configurations { get; set; }
    public string account_number { get; set; }
    public string status { get; set; }
    public string crypto_status { get; set; }
    public int options_approved_level { get; set; }
    public int options_trading_level { get; set; }
    public string currency { get; set; }
    public string buying_power { get; set; }
    public string regt_buying_power { get; set; }
    public string daytrading_buying_power { get; set; }
    public string effective_buying_power { get; set; }
    public string non_marginable_buying_power { get; set; }
    public string options_buying_power { get; set; }
    public string bod_dtbp { get; set; }
    public string cash { get; set; }
    public string accrued_fees { get; set; }
    public string portfolio_value { get; set; }
    public bool pattern_day_trader { get; set; }
    public bool trading_blocked { get; set; }
    public bool transfers_blocked { get; set; }
    public bool account_blocked { get; set; }
    public DateTime created_at { get; set; }
    public bool trade_suspended_by_user { get; set; }
    public string multiplier { get; set; }
    public bool shorting_enabled { get; set; }
    public string equity { get; set; }
    public string last_equity { get; set; }
    public string long_market_value { get; set; }
    public string short_market_value { get; set; }
    public string position_market_value { get; set; }
    public string initial_margin { get; set; }
    public string maintenance_margin { get; set; }
    public string last_maintenance_margin { get; set; }
    public string sma { get; set; }
    public int daytrade_count { get; set; }
    public string balance_asof { get; set; }
    public int crypto_tier { get; set; }
    public string intraday_adjustments { get; set; }
    public string pending_reg_taf_fees { get; set; }
    public class Admin_Configurations
    {
    }
}