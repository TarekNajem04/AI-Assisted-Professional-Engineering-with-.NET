// book/samples/Chapter-03/EngineeringCommunicationToolkit/src/AiToolkit.Application/Ports/IAiOutputParser.cs
namespace AiToolkit.Application.Ports;

public interface IAiOutputParser<T> { ParseResult<T> Parse(string raw); }

public sealed record ParseResult<T>
{
    public bool IsSuccess { get; private init; }
    public T? Value { get; private init; }
    public string? Error { get; private init; }
    public static ParseResult<T> Success(T value) => new() { IsSuccess = true, Value = value };
    public static ParseResult<T> Failure(string error) => new() { IsSuccess = false, Error = error };
}
