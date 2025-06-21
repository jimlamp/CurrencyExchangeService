using AspNetCoreRateLimit;
using CurremcyExchange.Core.Abstractions.Settings;
using CurrencyExchange.Core.Abstractions.Enums;
using CurrencyExchange.Core.Abstractions.Factories;
using CurrencyExchange.Core.Abstractions.Helpers;
using CurrencyExchange.Core.Abstractions.Managers;
using CurrencyExchange.Core.BackgroundJobs;
using CurrencyExchange.Core.Factories;
using CurrencyExchange.Core.Helpers;
using CurrencyExchange.Core.Managers;
using CurrencyExchange.Data;
using CurrencyExchange.Data.Implementations;
using CurrencyExchange.Data.Interfaces;
using CurrencyExchange.ECB.Gateway.Services.Implementations;
using CurrencyExchange.ECB.Gateway.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });

    c.MapType<SupportedFundStrategiesEnum>(() => new OpenApiSchema
    {
        Type = "string",
        Enum = Enum.GetNames(typeof(SupportedFundStrategiesEnum))
        .Select(name => new OpenApiString(name))
        .ToList<IOpenApiAny>()
    });
});

builder.Services.Configure<EcbSettings>(builder.Configuration.GetSection("EcbSettings"));

// Register DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PaymentsDB")));

builder.Services.AddSingleton<CurrencyRateUpdateJob>();

builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("CurrencyRateUpdateJob");
    q.AddJob<CurrencyRateUpdateJob>(opts => opts.WithIdentity("CurrencyRateUpdateJob"));
    q.AddTrigger(opts => opts
        .ForJob("CurrencyRateUpdateJob")
        .WithIdentity("CurrencyRateUpdateJobTrigger")
        .StartNow()
        .WithSimpleSchedule(x => x.WithIntervalInMinutes(1).RepeatForever()));
});

// Register HttpClient
builder.Services.AddHttpClient();

// Register Services
builder.Services.AddScoped<IEcbCurrencyServiceClient, EcbCurrencyServiceClient>();

// Register repositories
builder.Services.AddScoped<ICurrencyRateRepository, CurrencyRateRepository>();
builder.Services.AddScoped<IWalletRepository, WalletRepository>();

// Register managers
builder.Services.AddScoped<IWalletManager, WalletManager>();

//Register factories
builder.Services.AddScoped<IWalletStrategyFactory, WalletStrategyFactory>();

// Register helpers
builder.Services.AddScoped<IConversionRateHelper, ConversionRateHelper>();

//Register cache
builder.Services.Configure<CacheSettings>(builder.Configuration.GetSection("CacheSettings"));
builder.Services.AddMemoryCache();

builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));

// Add rate limiting dependencies
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddMemoryCache();
builder.Services.AddInMemoryRateLimiting();

var app = builder.Build();

app.UseIpRateLimiting();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using var scope = app.Services.CreateScope();
var scheduler = await scope.ServiceProvider.GetRequiredService<ISchedulerFactory>().GetScheduler();
await scheduler.Start();

app.Run();
