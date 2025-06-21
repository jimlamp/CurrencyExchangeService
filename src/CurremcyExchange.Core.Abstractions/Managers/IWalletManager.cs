using CurrencyExchange.Core.Abstractions.Enums;
using CurrencyExchange.Core.Abstractions.Models;
using CurrencyExchange.Data.Models;

namespace CurrencyExchange.Core.Abstractions.Managers
{
    public interface IWalletManager
    {
        Task<OperationResult<Wallet>> CreateWallet();
        Task<OperationResult<decimal>> GetBalance(int walletId, string currency);
        Task<OperationResult<Wallet>> AdjustBalance(int walletId, decimal amount, string currency, SupportedFundStrategiesEnum strategy);
    }
}
