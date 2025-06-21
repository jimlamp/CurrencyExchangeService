using CurrencyExchange.Core.Abstractions.Enums;
using CurrencyExchange.Core.Abstractions.Strategies;

namespace CurrencyExchange.Core.Abstractions.Factories
{
    public interface IWalletStrategyFactory
    {
        IWalletStrategy GetStrategy(SupportedFundStrategiesEnum strategy);
    }
}
