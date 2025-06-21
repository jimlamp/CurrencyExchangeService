using CurrencyExchange.Core.Abstractions.Strategies;
using CurrencyExchange.Data.Models;

namespace CurrencyExchange.Core.Strategies
{
    public class SubtractFundsStrategy : IWalletStrategy
    {
        public void Execute(Wallet wallet, decimal amount)
        {
            if (wallet.Balance < amount)
                throw new InvalidOperationException("Insufficient funds.");
            wallet.Balance -= amount;
        }
    }
}
