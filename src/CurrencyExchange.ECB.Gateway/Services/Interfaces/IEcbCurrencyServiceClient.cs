using CurrencyExchange.ECB.Gateway.Models;

namespace CurrencyExchange.ECB.Gateway.Services.Interfaces
{
    public interface IEcbCurrencyServiceClient
    {
        Task<EcbExchangeRatesResponse?> GetExchangeRatesResponseAsync(string ecbCurrenciesUrl, CancellationToken token = default);
    }
}
