using CurrencyExchange.Core.Helpers;
using CurrencyExchange.Data.Interfaces;
using Moq;
using Xunit;

namespace CurrencyExchangeWallet.Tests.Helpers
{
    [Trait("Category", "Unit")]
    public class ConversionRateHelperTests
    {
        private readonly Mock<ICurrencyRateRepository> _mockCurrencyRateRepository;
        private readonly ConversionRateHelper _conversionRateHelper;

        public ConversionRateHelperTests()
        {
            _mockCurrencyRateRepository = new Mock<ICurrencyRateRepository>();
            _conversionRateHelper = new ConversionRateHelper(_mockCurrencyRateRepository.Object);
        }

        [Fact]
        public async Task ConvertCurrency_ShouldConvert_WhenFromCurrencyIsBaseCurrency()
        {
            // Arrange
            var amount = 100m;
            var fromCurrency = "EUR";
            var toCurrency = "USD";
            var exchangeRates = new Dictionary<string, decimal>
            {
                { "USD", 1.2m }
            };
            _mockCurrencyRateRepository
                .Setup(repo => repo.GetLatestRatesAsync())
                .ReturnsAsync(exchangeRates);

            // Act
            var result = await _conversionRateHelper.ConvertCurrency(amount, fromCurrency, toCurrency);

            // Assert
            Assert.Equal(120m, result);
        }

        [Fact]
        public async Task ConvertCurrency_ShouldConvert_WhenBothCurrenciesAreNotBaseCurrency()
        {
            // Arrange
            var amount = 100m;
            var fromCurrency = "USD";
            var toCurrency = "GBP";
            var exchangeRates = new Dictionary<string, decimal>
            {
                { "USD", 1.2m },
                { "GBP", 0.8m }
            };
            _mockCurrencyRateRepository
                .Setup(repo => repo.GetLatestRatesAsync())
                .ReturnsAsync(exchangeRates);

            // Act
            var result = await _conversionRateHelper.ConvertCurrency(amount, fromCurrency, toCurrency);

            // Assert
            var expected = Math.Round((amount / 1.2m) * 0.8m, 2, MidpointRounding.AwayFromZero);
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task ConvertCurrency_ShouldThrowInvalidOperationException_WhenExchangeRateIsNotAvailable()
        {
            // Arrange
            var amount = 100m;
            var fromCurrency = "USD";
            var toCurrency = "AUD";
            var exchangeRates = new Dictionary<string, decimal>
            {
                { "USD", 1.2m }
            };
            _mockCurrencyRateRepository
                .Setup(repo => repo.GetLatestRatesAsync())
                .ReturnsAsync(exchangeRates);

            // Act
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _conversionRateHelper.ConvertCurrency(amount, fromCurrency, toCurrency)
            );

            //Assert
            Assert.Equal("Exchange rate not available for this conversion.", exception.Message);
        }

        [Fact]
        public async Task ConvertCurrency_ShouldThrowInvalidOperationException_WhenExchangeRateIsMissingForBothCurrencies()
        {
            // Arrange
            var amount = 100m;
            var fromCurrency = "USD";
            var toCurrency = "GBP";
            var exchangeRates = new Dictionary<string, decimal>();
            _mockCurrencyRateRepository
                .Setup(repo => repo.GetLatestRatesAsync())
                .ReturnsAsync(exchangeRates);

            // Act
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _conversionRateHelper.ConvertCurrency(amount, fromCurrency, toCurrency)
            );

            //Assert
            Assert.Equal("Exchange rate not available for this conversion.", exception.Message);
        }
    }
}
