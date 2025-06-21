using CurrencyExchange.Data.Models;

namespace CurrencyExchange.Core.Abstractions.Strategies
{
    public interface IWalletStrategy
    {
        void Execute(Wallet wallet, decimal amount);
    }
}
