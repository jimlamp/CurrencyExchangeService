namespace CurrencyExchange.Core.Abstractions.Helpers
{
    public interface IConversionRateHelper
    {
        Task<decimal> ConvertCurrency(decimal amount, string fromCurrency, string toCurrency);
    }
}
