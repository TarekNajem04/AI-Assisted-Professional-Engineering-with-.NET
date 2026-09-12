// book/samples/Chapter-01/DistributedPaymentService/src/PaymentService.Application/Ports/IPaymentGateway.cs
using PaymentService.Core.Domain;

namespace PaymentService.Application.Ports;

public interface IPaymentGateway
{
    Task<GatewayResponse> ChargeAsync(Money amount, PaymentMethod method, CancellationToken ct);
}

public sealed record GatewayResponse(bool Success, string TransactionId, string? ErrorCode);
