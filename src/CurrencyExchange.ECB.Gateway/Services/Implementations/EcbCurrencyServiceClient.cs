using CurrencyExchange.ECB.Gateway.Models;
using CurrencyExchange.ECB.Gateway.Services.Helpers;
using CurrencyExchange.ECB.Gateway.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Xml.Serialization;

namespace CurrencyExchange.ECB.Gateway.Services.Implementations
{
    public class EcbCurrencyServiceClient : IEcbCurrencyServiceClient
    {
        public const string ClientId = "ECB.Client";

        private readonly ILogger<EcbCurrencyServiceClient> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public EcbCurrencyServiceClient(ILogger<EcbCurrencyServiceClient> logger,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<EcbExchangeRatesResponse?> GetExchangeRatesResponseAsync(string ecbCurrenciesUrl, CancellationToken token = default)
        {
            _logger.LogInformation($"[{ClientId}] - {nameof(GetExchangeRatesResponseAsync)} new request");

            try
            {
                if(string.IsNullOrEmpty(ecbCurrenciesUrl))
                {
                    throw new ArgumentNullException(nameof(ecbCurrenciesUrl));
                }

                HttpClient httpClient = _httpClientFactory.CreateClient(ClientId);

                using HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, ecbCurrenciesUrl);
                using HttpResponseMessage httpResponse = await httpClient.SendAsync(httpRequest);

                string content = await httpResponse.Content.ReadAsStringAsync(token);

                if (!httpResponse.IsSuccessStatusCode)
                {
                    _logger.LogError($"[{ClientId}] - {nameof(GetExchangeRatesResponseAsync)} failed with StatusCode: {httpResponse.StatusCode}, response: {content}");

                    return new EcbExchangeRatesResponse
                    {
                        HasError = true
                    };
                }

                _logger.LogInformation($"[{ClientId}] - {nameof(GetExchangeRatesResponseAsync)} response: {content}");

                var serializer = new XmlSerializer(typeof(Envelope));

                using var stringReader = new StringReader(content);

                Envelope? ecbData = serializer.Deserialize(new IgnoreNamespaceXmlTextReader(stringReader)) as Envelope;

                return new EcbExchangeRatesResponse {
                    Envelope = ecbData
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting exchange rates from ECB service.");
                throw;
            }
        }
    }
}
