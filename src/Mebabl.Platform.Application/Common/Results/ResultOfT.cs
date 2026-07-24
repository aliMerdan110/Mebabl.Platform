namespace Mebabl.Platform.Application.Common.Results;

public class Result<T> : Result
{
    public T? Value { get; }

    private Result(
        bool succeeded,
        T? value,
        string? error)
        : base(succeeded, error)
    {
        Value = value;
    }

    public static Result<T> Success(T value)
        => new(true, value, null);

    public static new Result<T> Failure(string error)
        => new(false, default, error);
}