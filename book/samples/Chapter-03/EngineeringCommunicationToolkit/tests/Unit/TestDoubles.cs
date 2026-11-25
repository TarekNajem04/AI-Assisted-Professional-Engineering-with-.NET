// book/samples/Chapter-03/EngineeringCommunicationToolkit/tests/Unit/TestDoubles.cs
//
// Shared deterministic test doubles. No live AI call is used anywhere in the sample tests.

using System.Runtime.CompilerServices;
using AiToolkit.Application.Ports;

namespace AiToolkit.Tests.Unit;

internal sealed class StubChatClient(
    Func<string, string, CancellationToken, Task<string>> handler)
    : IChatCompletionClient
{
    public Task<string> CompleteAsync(
        string systemPrompt, string userPrompt, CancellationToken ct) =>
        handler(systemPrompt, userPrompt, ct);

    public async IAsyncEnumerable<string> CompleteStreamingAsync(
        string systemPrompt,
        string userPrompt,
        [EnumeratorCancellation] CancellationToken ct)
    {
        yield return await CompleteAsync(systemPrompt, userPrompt, ct);
    }
}

internal sealed class FailOnceParser : IAiOutputParser<string>
{
    public int Calls { get; private set; }

    public ParseResult<string> Parse(string raw)
    {
        Calls++;
        return Calls == 1
            ? ParseResult<string>.Failure("Simulated malformed output.")
            : ParseResult<string>.Success(raw);
    }
}
