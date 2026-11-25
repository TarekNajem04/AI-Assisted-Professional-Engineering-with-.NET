// book/samples/Chapter-03/EngineeringCommunicationToolkit/src/AiToolkit.Application/Ports/IChatCompletionClient.cs
namespace AiToolkit.Application.Ports;

public interface IChatCompletionClient
{
    Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken ct);
    IAsyncEnumerable<string> CompleteStreamingAsync(string systemPrompt, string userPrompt, CancellationToken ct);
}
