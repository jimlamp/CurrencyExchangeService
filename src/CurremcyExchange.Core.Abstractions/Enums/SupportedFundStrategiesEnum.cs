using System.Runtime.Serialization;

namespace CurrencyExchange.Core.Abstractions.Enums
{
    public enum SupportedFundStrategiesEnum
    {
        [EnumMember(Value = "AddFunds")]
        AddFunds,
        [EnumMember(Value = "SubtractFunds")]
        SubtractFunds,
        [EnumMember(Value = "ForceSubtractFunds")]
        ForceSubtractFunds
    }
}
