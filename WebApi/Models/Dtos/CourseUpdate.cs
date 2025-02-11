namespace WebApi.Models.Dtos;

public class CourseUpdate
{
    public int CourseId { get; set; }

    public required string Title { get; set; }

    public int Credits { get; set; }

    public Result ValidateAsync()
    {
        if (CourseId != 0)
        {
            return CourseUpdateError.IdNotUnique;
        }

        if (string.IsNullOrEmpty(Title))
        {
            return CourseUpdateError.TitleEmpty;
        }

        if (Credits < 0 || Credits > 5)
        {
            return CourseUpdateError.OutOfRange;
        }

        return Result.Success();
    }
}

public record Error(string code, string? description = null)
{
    public static readonly Error None = new (string.Empty);
}

public static class CourseUpdateError
{
    public static Error IdNotUnique => 
        new("CourseUpdate.IdNotUnique", "The CourseId field must required 0.");

    public static Error TitleEmpty =>
        new("CourseUpdate.TitleEmpty", "The Title field is required.");

    public static Error OutOfRange =>
        new("CourseUpdate.OutOfRange", "The Credits field must be between 0 and 5.");
}

public class Result
{
    public Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None || !isSuccess && error == Error.None)
        {
            throw new ArgumentException("Success result must not have an error.", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success() => new (true, Error.None);
    public static Result Failure(Error error) => new (false, error);

    public static implicit operator Result(Error error) => Failure(error);
}


