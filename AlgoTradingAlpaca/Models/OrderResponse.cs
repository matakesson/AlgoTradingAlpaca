using System.Text.Json.Serialization;

namespace AlgoTradingAlpaca.Models;

 public class OrderResponse
 {
     [JsonPropertyName("id")]
     public string Id { get; set; }

     [JsonPropertyName("client_order_id")]
     public string ClientOrderId { get; set; }

     [JsonPropertyName("created_at")]
     public DateTime CreatedAt { get; set; }

     [JsonPropertyName("updated_at")]
     public DateTime UpdatedAt { get; set; }

     [JsonPropertyName("submitted_at")]
     public DateTime SubmittedAt { get; set; }

     [JsonPropertyName("filled_at")]
     public DateTime? FilledAt { get; set; }

     [JsonPropertyName("expired_at")]
     public DateTime? ExpiredAt { get; set; }

     [JsonPropertyName("canceled_at")]
     public DateTime? CanceledAt { get; set; }

     [JsonPropertyName("failed_at")]
     public DateTime? FailedAt { get; set; }

     [JsonPropertyName("replaced_at")]
     public DateTime? ReplacedAt { get; set; }

     [JsonPropertyName("replaced_by")]
     public string ReplacedBy { get; set; }

     [JsonPropertyName("replaces")]
     public string Replaces { get; set; }

     [JsonPropertyName("asset_id")]
     public string AssetId { get; set; }

     [JsonPropertyName("symbol")]
     public string Symbol { get; set; }

     [JsonPropertyName("asset_class")]
     public string AssetClass { get; set; }

     [JsonPropertyName("notional")]
     public decimal? Notional { get; set; }

     [JsonPropertyName("qty")]
     public string Qty { get; set; }

     [JsonPropertyName("filled_qty")]
     public string FilledQty { get; set; }

     [JsonPropertyName("filled_avg_price")]
     public decimal? FilledAvgPrice { get; set; }

     [JsonPropertyName("order_class")]
     public string OrderClass { get; set; }

     [JsonPropertyName("order_type")]
     public string OrderType { get; set; }

     [JsonPropertyName("type")]
     public string Type { get; set; }

     [JsonPropertyName("side")]
     public string Side { get; set; }

     [JsonPropertyName("position_intent")]
     public string PositionIntent { get; set; }

     [JsonPropertyName("time_in_force")]
     public string TimeInForce { get; set; }

     [JsonPropertyName("limit_price")]
     public decimal? LimitPrice { get; set; }

     [JsonPropertyName("stop_price")]
     public decimal? StopPrice { get; set; }

     [JsonPropertyName("status")]
     public string Status { get; set; }

     [JsonPropertyName("extended_hours")]
     public bool ExtendedHours { get; set; }

     [JsonPropertyName("legs")]
     public object Legs { get; set; }

     [JsonPropertyName("trail_percent")]
     public decimal? TrailPercent { get; set; }

     [JsonPropertyName("trail_price")]
     public decimal? TrailPrice { get; set; }

     [JsonPropertyName("hwm")]
     public decimal? Hwm { get; set; }

     [JsonPropertyName("subtag")]
     public string Subtag { get; set; }

     [JsonPropertyName("source")]
     public string Source { get; set; }

     [JsonPropertyName("expires_at")]
     public DateTime? ExpiresAt { get; set; }
 }