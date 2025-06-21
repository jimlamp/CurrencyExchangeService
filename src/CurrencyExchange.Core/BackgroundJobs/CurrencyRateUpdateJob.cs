using CurremcyExchange.Core.Abstractions.Settings;
using CurrencyExchange.Data.Interfaces;
using CurrencyExchange.Data.Models;
using CurrencyExchange.ECB.Gateway.Models;
using CurrencyExchange.ECB.Gateway.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;

namespace CurrencyExchange.Core.BackgroundJobs
{
    public class CurrencyRateUpdateJob : IJob
    {
        private readonly IOptions<EcbSettings> _options;
        private readonly ILogger<CurrencyRateUpdateJob> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public CurrencyRateUpdateJob(ILogger<CurrencyRateUpdateJob> logger,
            IServiceScopeFactory serviceScopeFactory,
            IOptions<EcbSettings> options)
        {
            _logger = logger;
            _options = options;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation($"[{nameof(CurrencyRateUpdateJob)}] - {nameof(Execute)} new request");

            using var scope = _serviceScopeFactory.CreateScope();
            var serviceProvider = scope.ServiceProvider;

            var ecbCurrencyServiceClient = serviceProvider.GetRequiredService<IEcbCurrencyServiceClient>();
            var currencyRateRepository = serviceProvider.GetRequiredService<ICurrencyRateRepository>();

            EcbExchangeRatesResponse? exchangeRatesResponse = await ecbCurrencyServiceClient.GetExchangeRatesResponseAsync(_options.Value.BaseUrl);

            if (exchangeRatesResponse == null || exchangeRatesResponse.HasError)
            {
                _logger.LogError($"[{nameof(CurrencyRateUpdateJob)}] - {nameof(Execute)} failed to get exchange rates response");
                return;
            }

            var timeCubes = exchangeRatesResponse.Envelope?.CubeRoot?.TimeCubes.FirstOrDefault();

            if (timeCubes == null || !timeCubes.Rates.Any())
            {
                _logger.LogError($"[{nameof(CurrencyRateUpdateJob)}] - {nameof(Execute)} rates list is empty");
                return;
            }

            List<ExchangeRate> exchangeRates = timeCubes.Rates
                .Select(rate => new ExchangeRate
                {
                    Currency = rate.Currency,
                    Rate = rate.Rate,
                    CreatedDate = DateTime.Parse(timeCubes.Time.ToString())
                })
                .ToList();

            bool result = await currencyRateRepository.InsertOrUpdateCurrencyRatesWithMergeAsync(exchangeRates);

            _logger.LogInformation($"[{nameof(CurrencyRateUpdateJob)}] - {nameof(Execute)} operation result: {result}");
        }
    }
}
