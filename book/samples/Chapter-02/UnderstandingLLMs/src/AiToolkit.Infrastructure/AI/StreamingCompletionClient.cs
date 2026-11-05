// book/samples/Chapter-02/UnderstandingLLMs/src/AiToolkit.Infrastructure/AI/StreamingCompletionClient.cs
//
// S01 concept: makes the per-token, sequential nature of generation
// directly observable. Implements IChatCompletionClient against the
// OpenAI SDK. Every chunk yielded corresponds to one or a small number
// of tokens emitted by ONE forward pass through the model — there is no
// separate "planning" pass that runs before this loop begins (Section 01).

using System.ClientModel;
using AiToolkit.Application.Ports;
using OpenAI.Chat;

namespace AiToolkit.Infrastructure.AI;

public sealed class StreamingCompletionClient : IChatCompletionClient {
    private readonly ChatClient _client;

    public StreamingCompletionClient(string apiKey, string model = "gpt-4o-mini") {
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);
        _client = new ChatClient(model, new ApiKeyCredential(apiKey));
    }

    public async Task<string> CompleteAsync(
        string systemPrompt, string userPrompt, CancellationToken ct) {
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
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct) {
        ChatMessage[] messages =
        [
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userPrompt),
        ];

        // Architecture note (S01): each `update` below corresponds to
        // ONE forward pass through the transformer stack, conditioned on
        // the prompt PLUS every token generated so far in this loop.
        // Nothing here represents a pre-computed plan — structure that
        // emerges in the final text emerged token-by-token, in real time.
        await foreach(var update in _client.CompleteChatStreamingAsync(messages, cancellationToken: ct)) {
            foreach(var part in update.ContentUpdate) {
                if(!string.IsNullOrEmpty(part.Text)) {
                    yield return part.Text;
                }
            }
        }
    }
}
