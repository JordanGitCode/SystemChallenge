namespace SystemChallengeAPI.Domain
{
    public enum OperationStatus
    {
        Success,
        NotFound,
        Forbidden,
        InvalidTransition
    }

    public class OperationResult<T>
    {
        public OperationStatus Status { get; init; }
        public T? Value { get; init; }
        public string? Error { get; init; }

        public static OperationResult<T> Ok(T value) => new() { Status = OperationStatus.Success, Value = value };
        public static OperationResult<T> NotFound(string e) => new() { Status = OperationStatus.NotFound, Error = e };
        public static OperationResult<T> Forbidden(string e) => new() { Status = OperationStatus.Forbidden, Error = e };
        public static OperationResult<T> Invalid(string e) => new() { Status = OperationStatus.InvalidTransition, Error = e };
    }
}
