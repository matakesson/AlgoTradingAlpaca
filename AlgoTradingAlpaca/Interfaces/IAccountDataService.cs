using AlgoTradingAlpaca.Models;

namespace AlgoTradingAlpaca.Interfaces;

public interface IAccountDataService
{
    Task<Account> GetAccountAsync();
}