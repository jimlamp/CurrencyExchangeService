using CurrencyExchange.Core.Abstractions.Helpers;
using CurrencyExchange.Data.Interfaces;

namespace CurrencyExchange.Core.Helpers
{
    public class ConversionRateHelper : IConversionRateHelper
    {
        private readonly ICurrencyRateRepository _currencyRateRepository;

        public ConversionRateHelper(ICurrencyRateRepository currencyRateRepository)
        {
            _currencyRateRepository = currencyRateRepository;
        }

        public async Task<decimal> ConvertCurrency(decimal amount, string fromCurrency, string toCurrency)
        {
            const string baseCurrency = "EUR";

            if(fromCurrency == toCurrency)
            {
                return amount;
            }

            var exchangeRates = await _currencyRateRepository.GetLatestRatesAsync();

            if (fromCurrency == baseCurrency && exchangeRates.TryGetValue(toCurrency, out decimal directRate))
            {
                return Math.Round(amount * directRate, 2, MidpointRounding.AwayFromZero);
            }

            if (toCurrency == baseCurrency && exchangeRates.TryGetValue(fromCurrency, out decimal inverseRate))
            {
                return Math.Round(amount / inverseRate, 2, MidpointRounding.AwayFromZero);
            }

            if (exchangeRates.TryGetValue(fromCurrency, out decimal fromRate) &&
                exchangeRates.TryGetValue(toCurrency, out decimal toRate))
            {
                decimal amountInBaseCurrency = amount / fromRate;
                return Math.Round(amountInBaseCurrency * toRate, 2, MidpointRounding.AwayFromZero);
            }

            throw new InvalidOperationException("Exchange rate not available for this conversion.");
        }
    }
}
