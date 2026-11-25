// book/samples/Chapter-03/EngineeringCommunicationToolkit/src/AiToolkit.Infrastructure/AI/SchemaConstrainedClient.cs
//
// S05 concept: wraps chat completion calls with structured output mode,
// constraining the model to emit JSON conforming to a declared schema.
// Layer 1 of the variance-reduction stack — narrows output SHAPE before
// generation completes, eliminating an entire category of structural
// validation failures. Implements IChatCompletionClient so workflows
// depend on the abstraction, not on which generation mode is used.

using AiToolkit.Application.Ports;
using OpenAI.Chat;

namespace AiToolkit.Infrastructure.AI;

public sealed class SchemaConstrainedClient(
    ChatClient openAiClient,
    BinaryData jsonSchema,
    string schemaName)
    : IChatCompletionClient
{
    public async Task<string> CompleteAsync(
        string systemPrompt, string userPrompt, CancellationToken ct)
    {
        var options = new ChatCompletionOptions
        {
            // Schema-constrained output: constrains the model at the TOKEN
            // level to only emit tokens consistent with the declared schema.
            // Stronger than "please respond in JSON" instructions — a
            // configured constraint is more reliable than a requested one.
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                jsonSchemaFormatName: schemaName,
                jsonSchema: jsonSchema,
                jsonSchemaIsStrict: true),

            // Layer 2: low temperature narrows token-selection randomness
            // within the schema-constrained shape (Chapter 02 S05 caveat:
            // reduces variance, does not eliminate it).
            Temperature = 0.1f,
        };

        ChatMessage[] messages =
        [
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userPrompt),
        ];

        var response = await openAiClient.CompleteChatAsync(messages, options, ct);
        return response.Value.Content.Count > 0
            ? response.Value.Content[0].Text
            : string.Empty;
    }

    public async IAsyncEnumerable<string> CompleteStreamingAsync(
        string systemPrompt,
        string userPrompt,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
    {
        // Schema-constrained generation with streaming: omitted here for
        // clarity — schema-constrained workflows use the non-streaming path
        // because validation requires the complete schema-conformant response.
        yield return await CompleteAsync(systemPrompt, userPrompt, ct);
    }
}

// ── Example schema for CodeReviewPromptContract output ───────────────────

public static class CodeReviewSchema
{
    public static readonly BinaryData Schema = BinaryData.FromString(
        """
        {
          "type": "object",
          "properties": {
            "issues": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "line":        { "type": "integer" },
                  "severity":    { "type": "string", "enum": ["info","warning","error"] },
                  "description": { "type": "string" }
                },
                "required": ["line","severity","description"],
                "additionalProperties": false
              }
            }
          },
          "required": ["issues"],
          "additionalProperties": false
        }
        """);
}
