using AlgoTradingAlpaca.Models;

namespace AlgoTradingAlpaca.Interfaces;

public interface ITradingStrategy
{
    public Task ExecuteTradingStrategy();
}