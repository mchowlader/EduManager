namespace EduSystem.Identity.Shared.Common;

/// <summary>
/// Represents the result of an operation without return data
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? ErrorMessage { get; protected set; }
    public List<string> Errors { get; protected set; }

    protected Result(bool isSuccess, string? errorMessage = null, List<string>? error = null)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        Errors = error ?? new List<string>();

        if (errorMessage != null && !Errors.Contains(errorMessage))
        {
            Errors.Add(errorMessage);
        }
    }

    public static Result Success()
        => new Result(true);

    public static Result Failure(string errorMessage)
        => new Result(false, errorMessage);

    public static Result Failure(List<string> errors)
        => new Result(false, errors.FirstOrDefault(), errors);
}

/// <summary>
/// Represents the result of an operation with return data
/// </summary>
/// <typeparam name="T">The type of data returned</typeparam>

public class Result<T> : Result
{
    public T? Data { get; }

    private Result(bool isSuccess, T? data, string? errorMessage = null, List<string>? errors = null)
        : base(isSuccess, errorMessage, errors)
    {
        Data = data;
    }

    public static Result<T> Success(T data)
        => new Result<T>(true, data);

    public static new Result<T> Failure(string errorMessage)
        => new Result<T>(false, default, errorMessage);

    public static new Result<T> Failure(List<string> errors)
        => new Result<T>(false, default, errors.FirstOrDefault(), errors);


    /// <summary>
    /// Maps the result data to a new type
    /// </summary>
    public Result<TNew> Map<TNew>(Func<T?, TNew> mapper)
    {
        return IsSuccess
            ? Result<TNew>.Success(mapper(Data))
            : Result<TNew>.Failure(Errors);
    }
}