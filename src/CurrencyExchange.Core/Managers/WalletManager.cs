using CurrencyExchange.Core.Abstractions.Enums;
using CurrencyExchange.Core.Abstractions.Factories;
using CurrencyExchange.Core.Abstractions.Helpers;
using CurrencyExchange.Core.Abstractions.Managers;
using CurrencyExchange.Core.Abstractions.Models;
using CurrencyExchange.Data.Interfaces;
using CurrencyExchange.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CurrencyExchange.Core.Managers
{
    public class WalletManager : IWalletManager
    {
        private readonly ILogger<WalletManager> _logger;
        private readonly IWalletStrategyFactory _strategyFactory;
        private readonly IWalletRepository _walletRepository;
        private readonly IConversionRateHelper _conversionRateHelper;


        public WalletManager(ILogger<WalletManager> logger,
            IWalletStrategyFactory strategyFactory,
            IWalletRepository walletRepository,
            IConversionRateHelper conversionRateHelper) 
        {
            _logger = logger;
            _strategyFactory = strategyFactory;
            _walletRepository = walletRepository;
            _conversionRateHelper = conversionRateHelper;
        }

        public async Task<OperationResult<Wallet>> AdjustBalance(int walletId, decimal amount, string currency,
            SupportedFundStrategiesEnum strategy)
        {
            if (amount <= 0)
            {
                return OperationResult<Wallet>.Failure("Amount must be positive.");
            }

            try
            {
                Wallet? wallet = await _walletRepository.GetWalletByIdAsync(walletId);
                if (wallet == null)
                {
                    _logger.LogWarning("Wallet with ID {WalletId} not found.", walletId);
                    return OperationResult<Wallet>.Failure("Wallet not found.");
                }

                decimal newBalance;
                try
                {
                    newBalance = await _conversionRateHelper.ConvertCurrency(wallet.Balance, wallet.Currency, currency);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Currency conversion failed from {FromCurrency} to {ToCurrency}", wallet.Currency, currency);
                    return OperationResult<Wallet>.Failure("Failed to convert currency.");
                }

                wallet.Balance = newBalance;
                wallet.Currency = currency;

                var strategyInstance = _strategyFactory.GetStrategy(strategy);
                strategyInstance.Execute(wallet, amount);

                _walletRepository.UpdateWallet(wallet);
                await _walletRepository.SaveChangesAsync();

                _logger.LogInformation("Wallet {WalletId} successfully adjusted by {Amount} {Currency} using {Strategy}.",
                    walletId, amount, currency, strategy);

                return OperationResult<Wallet>.Success(wallet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while adjusting wallet balance for Wallet ID {WalletId}.", walletId);
                return OperationResult<Wallet>.Failure("An unexpected error occurred. Please try again later.");
            }
        }

        public async Task<OperationResult<Wallet>> CreateWallet()
        {
            try
            {
                var wallet = new Wallet
                {
                    Balance = 0,
                    Currency = "EUR"
                };

                await _walletRepository.AddWalletAsync(wallet);
                await _walletRepository.SaveChangesAsync();

                return OperationResult<Wallet>.Success(wallet);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database update failed while creating wallet.");
                return OperationResult<Wallet>.Failure("A database error occurred while creating the wallet.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while creating wallet.");
                return OperationResult<Wallet>.Failure("An unexpected error occurred. Please try again later.");
            }
        }

        public async Task<OperationResult<decimal>> GetBalance(int walletId, string currency)
        {
            Wallet? wallet = await _walletRepository.GetWalletByIdAsync(walletId);

            if (wallet == null)
            {
                return OperationResult<decimal>.Failure("Wallet not found.");
            }

            decimal newBalance;
            try
            {
                newBalance = await _conversionRateHelper.ConvertCurrency(wallet.Balance, wallet.Currency, currency);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Currency conversion failed from {FromCurrency} to {ToCurrency}", wallet.Currency, currency);
                return OperationResult<decimal>.Failure("Failed to convert currency.");
            }

            _logger.LogInformation("Converted balance from {0} to {1} for wallet {2}.", wallet.Currency, currency, walletId);

            return OperationResult<decimal>.Success(newBalance);
        }
    }
}
