// Port interface for parsing/validating raw AI completion text into a
// typed result. Used by DeterministicAiBoundary<T> (S05's deterministic
// seam) — every concrete parser implements this to define what
// "structurally and semantically valid" means for its specific output type.

namespace AiToolkit.Application.Ports;

public interface IAiOutputParser<T> {
    ParseResult<T> Parse(string raw);
}

/// <summary>
/// Result type for IAiOutputParser<T>. Encapsulates success/failure of parsing
/// </summary>
/// <typeparam name="T">The type of the parsed result.</typeparam>
public sealed record ParseResult<T> {
    public bool IsSuccess { get; private init; }
    public T? Value { get; private init; }
    public string? Error { get; private init; }

    public static ParseResult<T> Success(T value) =>
        new() { IsSuccess = true, Value = value };

    public static ParseResult<T> Failure(string error) =>
        new() { IsSuccess = false, Error = error };
}
