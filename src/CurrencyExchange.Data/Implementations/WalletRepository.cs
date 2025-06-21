using CurrencyExchange.Data.Interfaces;
using CurrencyExchange.Data.Models;

namespace CurrencyExchange.Data.Implementations
{
    public class WalletRepository : IWalletRepository
    {
        private readonly AppDbContext _dbContext;

        public WalletRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddWalletAsync(Wallet wallet)
        {
           await _dbContext.Wallets.AddAsync(wallet);
        }

        public async Task<Wallet?> GetWalletByIdAsync(int walletId)
        {
            return await _dbContext.Wallets.FindAsync(walletId);
        }

        public void UpdateWallet(Wallet wallet)
        {
            _dbContext.Wallets.Update(wallet);
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
