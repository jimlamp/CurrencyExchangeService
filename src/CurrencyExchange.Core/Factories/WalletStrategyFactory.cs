using CurrencyExchange.Core.Abstractions.Enums;
using CurrencyExchange.Core.Abstractions.Factories;
using CurrencyExchange.Core.Abstractions.Strategies;
using CurrencyExchange.Core.Strategies;

namespace CurrencyExchange.Core.Factories
{
    public class WalletStrategyFactory : IWalletStrategyFactory
    {
        private readonly Dictionary<string, IWalletStrategy> _strategies = new()
        {
            { SupportedFundStrategiesEnum.AddFunds.ToString(), new AddFundsStrategy() },
            { SupportedFundStrategiesEnum.SubtractFunds.ToString(), new SubtractFundsStrategy() },
            { SupportedFundStrategiesEnum.ForceSubtractFunds.ToString(), new ForceSubtractFundsStrategy() }
        };

        public IWalletStrategy GetStrategy(SupportedFundStrategiesEnum strategy)
        {
            if (!_strategies.TryGetValue(strategy.ToString(), out var selectedStrategy))
                throw new InvalidOperationException("Invalid strategy.");

            return selectedStrategy;
        }
    }
}
