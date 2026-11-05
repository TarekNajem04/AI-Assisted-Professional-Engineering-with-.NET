// book/samples/Chapter-02/UnderstandingLLMs/src/AiToolkit.Application/ApplicationExtensions.cs

using Microsoft.Extensions.DependencyInjection;

namespace AiToolkit.Application;

/// <summary>
/// DI registration entry point for Application-layer services.
/// Currently a placeholder — Application in this sample is pure ports
/// (interfaces) with no concrete services of its own to register.
/// Kept for architectural symmetry with the Chapter 01 sample and as
/// the natural home for future use-case orchestration classes.
/// </summary>
public static class ApplicationExtensions {
    public static IServiceCollection AddApplication(this IServiceCollection services)
        => services;
}
