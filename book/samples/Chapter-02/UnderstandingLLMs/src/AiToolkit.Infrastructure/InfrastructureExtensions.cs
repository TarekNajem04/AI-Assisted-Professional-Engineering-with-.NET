using AiToolkit.Application.Ports;
using AiToolkit.Infrastructure.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AiToolkit.Infrastructure;

public static class InfrastructureExtensions {
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration) {
        ArgumentNullException.ThrowIfNull(configuration);
        // NOTE: appsettings.json ships "ApiKey": "" (empty, not missing), and
        // ?? does not treat empty as missing — so check whitespace explicitly.
        // Without a key the API still starts (the client factory is lazy),
        // but the first AI call fails. Fail here with a clear message instead
        // of a bare 500 + stack trace at first use.
        var apiKey = configuration["OpenAI:ApiKey"];
        if(string.IsNullOrWhiteSpace(apiKey)) {
            apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        }

        if(string.IsNullOrWhiteSpace(apiKey)) {
            throw new InvalidOperationException(
                "OPENAI_API_KEY not set. Set environment variable or " +
                "OpenAI:ApiKey in configuration.");
        }

        services.AddSingleton<IChatCompletionClient>(
            _ => new StreamingCompletionClient(apiKey));

        services.AddSingleton<IAiSuggestionValidator, AiSuggestionValidator>();
        services.AddSingleton<AiAuditLog>();

        // Concrete parser for the CodeSuggestion result type used by
        // AiSuggestionEndpoint. Additional IAiOutputParser<T> registrations
        // can be added here for other typed AI-backed endpoints.
        services.AddSingleton<IAiOutputParser<CodeSuggestion>, CodeSuggestionParser>();

        // Generic open type registration — DeterministicAiBoundary<T> can
        // be resolved for any T as long as an IAiOutputParser<T> exists.
        services.AddSingleton(typeof(DeterministicAiBoundary<>));

        return services;
    }
}
