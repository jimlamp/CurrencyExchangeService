using CurrencyExchange.Data.Models;

namespace CurrencyExchange.Data.Interfaces
{
    public interface ICurrencyRateRepository
    {
        Task<Dictionary<string, decimal>> GetLatestRatesAsync();
        Task<bool> InsertOrUpdateCurrencyRatesWithMergeAsync(List<ExchangeRate> exchangeRates);
    }
}
