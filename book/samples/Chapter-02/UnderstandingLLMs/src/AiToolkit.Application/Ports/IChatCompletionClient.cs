// Port interface — defined in Application, implemented in Infrastructure.
// Mirrors the architecture pattern from the Chapter 01 sample: Application
// depends on this abstraction, never on a concrete HTTP/SDK client.

namespace AiToolkit.Application.Ports;

/// <summary>
/// Port for chat completion calls. Abstracts the underlying SDK
/// (OpenAI, Microsoft.Extensions.AI, or any other provider) so that
/// Application and Api code never depend on a concrete client.
/// </summary>
public interface IChatCompletionClient {
    /// <summary>
    /// Non-streaming completion — returns the full response text once
    /// generation completes server-side. Used by DeterministicAiBoundary
    /// where validation must run against the complete output.
    /// </summary>
    /// <param name="systemPrompt">The system prompt for the chat completion.</param>
    /// <param name="userPrompt">The user prompt for the chat completion.</param>
    /// <param name="ct">The cancellation token.</param>
    Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken ct);

    /// <summary>
    /// Streaming completion — yields text chunks as they are generated.
    /// Makes Section 01's per-token, sequential generation mechanism
    /// directly observable to the caller.
    /// </summary>
    /// <param name="systemPrompt">The system prompt for the chat completion.</param>
    /// <param name="userPrompt">The user prompt for the chat completion.</param>
    /// <param name="ct">The cancellation token.</param>
    IAsyncEnumerable<string> CompleteStreamingAsync(string systemPrompt, string userPrompt, CancellationToken ct);
}
