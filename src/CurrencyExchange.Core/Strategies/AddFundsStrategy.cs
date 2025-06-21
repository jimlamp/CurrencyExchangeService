using CurrencyExchange.Core.Abstractions.Strategies;
using CurrencyExchange.Data.Models;

namespace CurrencyExchange.Core.Strategies
{
    public class AddFundsStrategy : IWalletStrategy
    {
        public void Execute(Wallet wallet, decimal amount)
        {
            wallet.Balance += amount;
        }
    }
}
