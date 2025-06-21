using CurrencyExchange.Data.Models;

namespace CurrencyExchange.Data.Interfaces
{
    public interface IWalletRepository
    {
        Task<Wallet?> GetWalletByIdAsync(int walletId);
        Task AddWalletAsync(Wallet wallet);
        Task SaveChangesAsync();
        void UpdateWallet(Wallet wallet);
    }
}
