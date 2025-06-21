using CurrencyExchange.Core.Abstractions.Enums;
using CurrencyExchange.Core.Abstractions.Factories;
using CurrencyExchange.Core.Abstractions.Helpers;
using CurrencyExchange.Core.Abstractions.Strategies;
using CurrencyExchange.Core.Managers;
using CurrencyExchange.Data.Interfaces;
using CurrencyExchange.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CurrencyExchangeWallet.Tests.Managers
{
    [Trait("Category", "Unit")]
    public class WalletManagerTests
    {
        private readonly Mock<IWalletRepository> _mockWalletRepo;
        private readonly Mock<IWalletStrategyFactory> _mockStrategyFactory;
        private readonly Mock<IConversionRateHelper> _mockConversionRateHelper;
        private readonly Mock<ILogger<WalletManager>> _mockLogger;
        private readonly WalletManager _walletManager;

        public WalletManagerTests()
        {
            _mockWalletRepo = new Mock<IWalletRepository>();
            _mockStrategyFactory = new Mock<IWalletStrategyFactory>();
            _mockConversionRateHelper = new Mock<IConversionRateHelper>();
            _mockLogger = new Mock<ILogger<WalletManager>>();
            _walletManager = new WalletManager(
                _mockLogger.Object,
                _mockStrategyFactory.Object,
                _mockWalletRepo.Object,
                _mockConversionRateHelper.Object
            );
        }

        [Fact]
        public async Task AdjustBalance_Success_ReturnsSuccessResult()
        {
            // Arrange
            var walletId = 1;
            var amount = 100m;
            var currency = "USD";
            var strategy = SupportedFundStrategiesEnum.AddFunds;

            var wallet = new Wallet { Id = walletId, Balance = 200m, Currency = "EUR" };
            _mockWalletRepo.Setup(repo => repo.GetWalletByIdAsync(walletId)).ReturnsAsync(wallet);
            _mockConversionRateHelper.Setup(helper => helper.ConvertCurrency(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(250m);

            var strategyMock = new Mock<IWalletStrategy>();
            _mockStrategyFactory.Setup(factory => factory.GetStrategy(strategy)).Returns(strategyMock.Object);

            // Act
            var result = await _walletManager.AdjustBalance(walletId, amount, currency, strategy);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(250m, wallet.Balance);
            _mockWalletRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task AdjustBalance_InvalidAmount_ReturnsFailureResult()
        {
            // Arrange
            var walletId = 1;
            var amount = -50m;
            var currency = "USD";
            var strategy = SupportedFundStrategiesEnum.SubtractFunds;

            // Act
            var result = await _walletManager.AdjustBalance(walletId, amount, currency, strategy);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Amount must be positive.", result.ErrorMessage);
            _mockWalletRepo.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task AdjustBalance_WalletNotFound_ReturnsFailureResult()
        {
            // Arrange
            var walletId = 900;
            var amount = 100m;
            var currency = "USD";
            var strategy = SupportedFundStrategiesEnum.AddFunds;

            _mockWalletRepo.Setup(repo => repo.GetWalletByIdAsync(walletId)).ReturnsAsync((Wallet?)null);

            // Act
            var result = await _walletManager.AdjustBalance(walletId, amount, currency, strategy);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Wallet not found.", result.ErrorMessage);
        }

        [Fact]
        public async Task AdjustBalance_CurrencyConversionFailed_ReturnsFailureResult()
        {
            // Arrange
            var walletId = 1;
            var amount = 100m;
            var currency = "USD";
            var strategy = SupportedFundStrategiesEnum.AddFunds;

            var wallet = new Wallet { Id = walletId, Balance = 200m, Currency = "EUR" };

            _mockWalletRepo.Setup(repo => repo.GetWalletByIdAsync(walletId)).ReturnsAsync(wallet);

            _mockConversionRateHelper.Setup(helper => helper.ConvertCurrency(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Conversion failed"));

            // Act
            var result = await _walletManager.AdjustBalance(walletId, amount, currency, strategy);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Failed to convert currency.", result.ErrorMessage);
        }

        [Fact]
        public async Task CreateWallet_Success_ReturnsSuccessResult()
        {
            // Arrange
            var wallet = new Wallet { Id = 1, Balance = 0, Currency = "EUR" };
            _mockWalletRepo.Setup(repo => repo.AddWalletAsync(It.IsAny<Wallet>())).Returns(Task.CompletedTask);
            _mockWalletRepo.Setup(repo => repo.SaveChangesAsync());

            // Act
            var result = await _walletManager.CreateWallet();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("EUR", result.Payload.Currency);
        }

        [Fact]
        public async Task CreateWallet_DbError_ReturnsFailureResult()
        {
            // Arrange
            _mockWalletRepo.Setup(repo => repo.AddWalletAsync(It.IsAny<Wallet>()))
                .Throws(new DbUpdateException("Database error"));

            // Act
            var result = await _walletManager.CreateWallet();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("A database error occurred while creating the wallet.", result.ErrorMessage);
        }

        [Fact]
        public async Task GetBalance_Success_ReturnsSuccessResult()
        {
            // Arrange
            var walletId = 1;
            var currency = "USD";

            var wallet = new Wallet { Id = walletId, Balance = 200m, Currency = "EUR" };

            _mockWalletRepo.Setup(repo => repo.GetWalletByIdAsync(walletId)).ReturnsAsync(wallet);

            _mockConversionRateHelper.Setup(helper => helper.ConvertCurrency(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(250m);

            // Act
            var result = await _walletManager.GetBalance(walletId, currency);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(250m, result.Payload);
        }

        [Fact]
        public async Task GetBalance_WalletNotFound_ReturnsFailureResult()
        {
            // Arrange
            var walletId = 999;
            var currency = "USD";
            _mockWalletRepo.Setup(repo => repo.GetWalletByIdAsync(walletId)).ReturnsAsync((Wallet?)null);

            // Act
            var result = await _walletManager.GetBalance(walletId, currency);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Wallet not found.", result.ErrorMessage);
        }
    }
}
