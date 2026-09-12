// book/samples/Chapter-01/DistributedPaymentService/src/PaymentService.Application/ApplicationExtensions.cs
using Microsoft.Extensions.DependencyInjection;
using PaymentService.Application.UseCases;

namespace PaymentService.Application;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<PaymentProcessor>();
        return services;
    }
}
