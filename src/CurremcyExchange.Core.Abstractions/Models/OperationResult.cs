namespace CurrencyExchange.Core.Abstractions.Models
{
    public class OperationResult<T>
    {
        public bool IsSuccess { get; }
        public string? ErrorMessage { get; }
        public T? Payload { get; }

        private OperationResult(bool isSuccess, string? errorMessage = null, T? payload = default)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
            Payload = payload;
        }

        public static OperationResult<T> Success(T payload) => new(true, null, payload);
        public static OperationResult<T> Success() => new(true);
        public static OperationResult<T> Failure(string errorMessage) => new(false, errorMessage);
    }
}
