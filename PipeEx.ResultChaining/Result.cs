namespace PipeEx.ResultChaining;

/// <summary>
/// A lightweight discriminated union that represents either a success (<typeparamref name="TSuccess"/>)
/// or a failure (<typeparamref name="TFailure"/>).
/// It is the carrier type used by the result chaining extension methods, so domain methods can be
/// chained without exceptions for control flow and without any external dependencies.
/// </summary>
/// <typeparam name="TSuccess">Represents success, also likely contains any required state/results for processing in the chain.</typeparam>
/// <typeparam name="TFailure">Represents a failure at some point in the chain.</typeparam>
public readonly struct Result<TSuccess, TFailure>
{
    private readonly TSuccess success;
    private readonly TFailure failure;

    private Result(bool isSuccess, TSuccess success, TFailure failure)
    {
        IsSuccess = isSuccess;
        this.success = success;
        this.failure = failure;
    }

    /// <summary>
    /// Gets a value indicating whether the result represents a success.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the result represents a failure.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the success value.
    /// </summary>
    /// <exception cref="InvalidOperationException">The result represents a failure.</exception>
    public TSuccess SuccessValue => IsSuccess ? success : throw new InvalidOperationException("The result represents a failure, not a success.");

    /// <summary>
    /// Gets the failure value.
    /// </summary>
    /// <exception cref="InvalidOperationException">The result represents a success.</exception>
    public TFailure FailureValue => IsFailure ? failure : throw new InvalidOperationException("The result represents a success, not a failure.");

    /// <summary>
    /// Creates a result that represents a success.
    /// </summary>
    /// <param name="value">The success value.</param>
    /// <returns>A successful result wrapping <paramref name="value"/>.</returns>
    public static Result<TSuccess, TFailure> Success(TSuccess value) => new(true, value, default!);

    /// <summary>
    /// Creates a result that represents a failure.
    /// </summary>
    /// <param name="failure">The failure value.</param>
    /// <returns>A failed result wrapping <paramref name="failure"/>.</returns>
    public static Result<TSuccess, TFailure> Failure(TFailure failure) => new(false, default!, failure);

    /// <summary>
    /// Implicitly wraps a success value in a result.
    /// </summary>
    /// <param name="value">The success value.</param>
    public static implicit operator Result<TSuccess, TFailure>(TSuccess value) => Success(value);

    /// <summary>
    /// Implicitly wraps a failure value in a result.
    /// </summary>
    /// <param name="failure">The failure value.</param>
    public static implicit operator Result<TSuccess, TFailure>(TFailure failure) => Failure(failure);

    /// <summary>
    /// Projects the result to a single value by invoking the matching transformation.
    /// </summary>
    /// <typeparam name="TResult">The type of the projection.</typeparam>
    /// <param name="onSuccess">The transformation applied when the result represents a success.</param>
    /// <param name="onFailure">The transformation applied when the result represents a failure.</param>
    /// <returns>The projected value.</returns>
    public TResult Match<TResult>(Func<TSuccess, TResult> onSuccess, Func<TFailure, TResult> onFailure) =>
        IsSuccess ? onSuccess(success) : onFailure(failure);

    /// <summary>
    /// Invokes the action that matches the state of the result.
    /// </summary>
    /// <param name="onSuccess">The action invoked when the result represents a success.</param>
    /// <param name="onFailure">The action invoked when the result represents a failure.</param>
    public void Switch(Action<TSuccess> onSuccess, Action<TFailure> onFailure)
    {
        if (IsSuccess) onSuccess(success);
        else onFailure(failure);
    }
}
