namespace AG.CLINICAL.Application.Common;

public sealed class Result<TValue>
{
    private Result(bool isSuccess, TValue? value, string? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public TValue? Value { get; }

    public string? Error { get; }

    public static Result<TValue> Success(TValue value)
    {
        return new Result<TValue>(true, value, null);
    }

    public static Result<TValue> Failure(string error)
    {
        return new Result<TValue>(false, default, error);
    }
}
