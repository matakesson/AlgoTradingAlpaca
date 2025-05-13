using AlgoTradingAlpaca.Models;

namespace AlgoTradingAlpaca.Interfaces;

public interface ICalculateBullishReversal
{
    public Task<TradeSignalResult?> CalculateBullishReversalSignal(List<BarData> barData);
}