namespace Mebabl.Platform.Application.Common.Results;

public class Result
{
    public bool Succeeded { get; }

    public string? Error { get; }

    protected Result(bool succeeded, string? error)
    {
        Succeeded = succeeded;
        Error = error;
    }

    public static Result Success()
    {
        return new Result(true, null);
    }

    public static Result Failure(string error)
    {
        return new Result(false, error);
    }
}