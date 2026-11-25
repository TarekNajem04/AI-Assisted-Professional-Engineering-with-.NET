// book/samples/Chapter-03/EngineeringCommunicationToolkit/src/AiToolkit.Infrastructure/InfrastructureExtensions.cs

using AiToolkit.Application.Ports;
using AiToolkit.Infrastructure.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AiToolkit.Infrastructure;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Verification seam: AiToolkit:ClientMode=Fake replaces SDK calls with
        // deterministic in-process clients. Never store a key in configuration.
        if (string.Equals(
                configuration["AiToolkit:ClientMode"],
                "Fake",
                StringComparison.OrdinalIgnoreCase))
        {
            var simulateFailure = string.Equals(
                configuration["AiToolkit:FakeFailure"],
                "true",
                StringComparison.OrdinalIgnoreCase);

            var freeFormOutput = configuration["AiToolkit:FakeFreeFormOutput"];
            var structuredOutput = configuration["AiToolkit:FakeStructuredOutput"];

            services.AddSingleton<IAiClientFactory>(
                new FakeAiClientFactory(
                    simulateFailure, freeFormOutput, structuredOutput));
        }
        else
        {
            var configuredKey = configuration["OpenAI:ApiKey"];
            var apiKey = string.IsNullOrWhiteSpace(configuredKey)
                ? Environment.GetEnvironmentVariable("OPENAI_API_KEY")
                : configuredKey;

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException(
                    "OPENAI_API_KEY not set. Set environment variable or " +
                    "OpenAI:ApiKey in configuration.");

            services.AddSingleton<IAiClientFactory>(
                new DefaultAiClientFactory(apiKey));
        }

        services.AddSingleton<IAiOutputParser<string>, PassThroughStringParser>();
        services.AddSingleton<ISpecDriftDetector, KeywordSpecDriftDetector>();

        // All workflows (S03), registered at one change point.
        services.AddWorkflows();

        return services;
    }
}
