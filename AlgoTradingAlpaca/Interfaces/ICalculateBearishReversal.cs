using AlgoTradingAlpaca.Models;

namespace AlgoTradingAlpaca.Interfaces;

public interface ICalculateBearishReversal
{
    public Task<TradeSignalResult?> CalculateBearishReversalSignal(List<BarData> barData);

}