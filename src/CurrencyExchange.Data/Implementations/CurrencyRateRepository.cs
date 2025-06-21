using CurrencyExchange.Data.Interfaces;
using CurrencyExchange.Data.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace CurrencyExchange.Data.Implementations
{
    public class CurrencyRateRepository : ICurrencyRateRepository
    {
        private readonly string _connectionString;
        private readonly AppDbContext _dbContext;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CurrencyRateRepository> _logger;
        private readonly IOptions<CacheSettings> _cacheSettings;

        public CurrencyRateRepository(IConfiguration configuration,
            AppDbContext dbContext,
            IOptions<CacheSettings> cacheSettings,
            IMemoryCache cache,
            ILogger<CurrencyRateRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("PaymentsDB")
                            ?? throw new ArgumentNullException(nameof(_connectionString)); ;
            _logger = logger;
            _cache = cache;
            _cacheSettings = cacheSettings;
            _dbContext = dbContext;
        }

        public async Task<Dictionary<string, decimal>> GetLatestRatesAsync()
        {
            if (_cache.TryGetValue(CacheKeys.LatestCurrencyRates, out Dictionary<string, decimal>? cachedRates))
            {
                return cachedRates!;
            }

            var latestRates = await ReadLatestRatesAsync();

            _cache.Set(CacheKeys.LatestCurrencyRates, latestRates, TimeSpan.FromMinutes(_cacheSettings.Value.CurrencyRatesCacheDurationInMinutes));

            return latestRates;
        }

        public async Task<bool> InsertOrUpdateCurrencyRatesWithMergeAsync(List<ExchangeRate> exchangeRates)
        {
            _logger.LogInformation($"[{nameof(CurrencyRateRepository)}] - {nameof(InsertOrUpdateCurrencyRatesWithMergeAsync)} starting operation");
            try
            {
                var mergeQuery = BuildMergeQuery(exchangeRates);

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var transaction = connection.BeginTransaction())
                    {
                        using (var command = new SqlCommand(mergeQuery, connection, transaction))
                        {
                            await command.ExecuteNonQueryAsync();
                        }

                        transaction.Commit();

                        var latestRates = await ReadLatestRatesAsync();

                        _cache.Set(CacheKeys.LatestCurrencyRates, latestRates, TimeSpan.FromMinutes(_cacheSettings.Value.CurrencyRatesCacheDurationInMinutes));

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{nameof(CurrencyRateRepository)}] - {nameof(InsertOrUpdateCurrencyRatesWithMergeAsync)} failed");
                return false;
            }
        }

        private async Task<Dictionary<string, decimal>> ReadLatestRatesAsync()
        {
            var latestRates = await _dbContext.ExchangeRates
                .GroupBy(r => r.Currency)
                .Select(g => new
                {
                    Currency = g.Key,
                    Rate = g.OrderByDescending(r => r.CreatedDate).Select(r => r.Rate).FirstOrDefault()
                })
                .ToDictionaryAsync(x => x.Currency, x => x.Rate);

            return latestRates;
        }

        private string BuildMergeQuery(List<ExchangeRate> rates)
        {
            var values = string.Join(", ", rates.Select(rate =>
            $"('{rate.Currency}', {rate.Rate.ToString(CultureInfo.InvariantCulture)}, '{rate.CreatedDate:yyyy-MM-dd}')"));

            var mergeQuery = $@"
            MERGE INTO dbo.ExchangeRates AS target
            USING (VALUES {values}) AS source (Currency, Rate, CreatedDate)
            ON target.Currency = source.Currency AND target.CreatedDate = source.CreatedDate
            WHEN MATCHED THEN
                UPDATE SET target.Rate = source.Rate,
                           target.UpdatedDate = GETUTCDATE()
            WHEN NOT MATCHED BY TARGET THEN
                INSERT (Currency, Rate, CreatedDate, UpdatedDate)
                VALUES (source.Currency, source.Rate, source.CreatedDate, NULL);";

            return mergeQuery;
        }
    }
}
