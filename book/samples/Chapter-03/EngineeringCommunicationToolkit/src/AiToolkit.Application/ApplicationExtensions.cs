// book/samples/Chapter-03/EngineeringCommunicationToolkit/src/AiToolkit.Application/ApplicationExtensions.cs
using Microsoft.Extensions.DependencyInjection;
namespace AiToolkit.Application;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services) => services;
}
