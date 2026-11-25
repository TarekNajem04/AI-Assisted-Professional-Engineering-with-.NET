// book/samples/Chapter-03/EngineeringCommunicationToolkit/src/AiToolkit.Infrastructure/AI/WorkflowRegistry.cs
// S03: single DI registration point for all workflows. The review workflow
// resolves the STRUCTURED client (S05); spec sessions use the free-form client.

using AiToolkit.Application.Ports;
using AiToolkit.Core.Prompts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AiToolkit.Infrastructure.AI;

public static class WorkflowRegistry
{
    public static IServiceCollection AddWorkflows(this IServiceCollection services)
    {
        services.AddSingleton<IAiWorkflow<CodeReviewRequest, string>>(sp =>
        {
            var client = sp.GetRequiredService<IAiClientFactory>().CreateStructured();
            var parser = sp.GetRequiredService<IAiOutputParser<string>>();
            var logger = sp.GetRequiredService<ILogger<DeterministicAiBoundary<string>>>();
            var boundary = new DeterministicAiBoundary<string>(client, parser, logger);

            return new AiWorkflow<CodeReviewRequest, string>(
                "code-review@1.0",
                new CodeReviewPromptContract(),
                request => new ContextBuilder(4000).Add(request.Code, 10, "code"),
                boundary);
        });

        return services;
    }
}

public sealed class PassThroughStringParser : IAiOutputParser<string>
{
    public ParseResult<string> Parse(string raw) =>
        string.IsNullOrWhiteSpace(raw)
            ? ParseResult<string>.Failure("Empty response.")
            : ParseResult<string>.Success(raw.Trim());
}
