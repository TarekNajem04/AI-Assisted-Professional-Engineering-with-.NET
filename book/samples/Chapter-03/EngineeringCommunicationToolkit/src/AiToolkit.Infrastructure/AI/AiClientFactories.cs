// book/samples/Chapter-03/EngineeringCommunicationToolkit/src/AiToolkit.Infrastructure/AI/AiClientFactories.cs
//
// Real and in-process-fake implementations of IAiClientFactory.
// The fake is a deliberate verification seam: it lets every chapter path,
// including failure handling, be executed deterministically without a key.
// It is selected only when AiToolkit:ClientMode=Fake is configured.

using System.Runtime.CompilerServices;
using AiToolkit.Application.Ports;
using OpenAI.Chat;

namespace AiToolkit.Infrastructure.AI;

internal sealed class DefaultAiClientFactory(string apiKey) : IAiClientFactory
{
    private const string Model = "gpt-4o-mini";

    private readonly Lazy<FreeFormChatClient> _freeForm =
        new(() => new FreeFormChatClient(Model, apiKey));

    private readonly Lazy<SchemaConstrainedClient> _structured =
        new(() => new SchemaConstrainedClient(
            openAiClient: new ChatClient(Model, apiKey),
            jsonSchema: CodeReviewSchema.Schema,
            schemaName: "code-review-result"));

    public IChatCompletionClient CreateFreeForm() => _freeForm.Value;
    public IChatCompletionClient CreateStructured() => _structured.Value;
}

/// <summary>
/// Deterministic in-process clients used for sample verification only.
/// No credentials, network calls, or machine-specific paths are involved.
/// </summary>
public sealed class FakeAiClientFactory(
    bool simulateFailure = false,
    string? freeFormOutput = null,
    string? structuredOutput = null) : IAiClientFactory
{
    private const string DefaultStructuredOutput =
        "{\"issues\":[{\"line\":1,\"severity\":\"warning\"," +
        "\"description\":\"Sample finding\"}]}";

    private const string DefaultFreeFormOutput =
        "C01: the output cites the active clause and stays consistent with it.";

    public IChatCompletionClient CreateFreeForm() =>
        new FakeChatCompletionClient(
            simulateFailure,
            freeFormOutput ?? DefaultFreeFormOutput);

    public IChatCompletionClient CreateStructured() =>
        new FakeChatCompletionClient(
            simulateFailure,
            structuredOutput ?? DefaultStructuredOutput);
}

internal sealed class FakeChatCompletionClient(bool simulateFailure, string output)
    : IChatCompletionClient
{
    public Task<string> CompleteAsync(
        string systemPrompt, string userPrompt, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (simulateFailure)
            throw new InvalidOperationException("Simulated provider failure.");

        return Task.FromResult(output);
    }

    public async IAsyncEnumerable<string> CompleteStreamingAsync(
        string systemPrompt,
        string userPrompt,
        [EnumeratorCancellation] CancellationToken ct)
    {
        yield return await CompleteAsync(systemPrompt, userPrompt, ct);
    }
}

internal sealed class FreeFormChatClient(string model, string apiKey)
    : IChatCompletionClient
{
    private readonly ChatClient _client = new(model, apiKey);

    public async Task<string> CompleteAsync(
        string systemPrompt, string userPrompt, CancellationToken ct)
    {
        ChatMessage[] messages =
        [
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userPrompt),
        ];

        var response = await _client.CompleteChatAsync(messages, cancellationToken: ct);
        return response.Value.Content.Count > 0
            ? response.Value.Content[0].Text
            : string.Empty;
    }

    public async IAsyncEnumerable<string> CompleteStreamingAsync(
        string systemPrompt,
        string userPrompt,
        [EnumeratorCancellation] CancellationToken ct)
    {
        await foreach (var update in _client.CompleteChatStreamingAsync(
            [new SystemChatMessage(systemPrompt), new UserChatMessage(userPrompt)],
            cancellationToken: ct))
        {
            foreach (var part in update.ContentUpdate)
                if (!string.IsNullOrEmpty(part.Text))
                    yield return part.Text;
        }
    }
}
