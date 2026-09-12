// book/samples/Chapter-01/DistributedPaymentService/src/PaymentService.Infrastructure/Gateway/StubGatewayClient.cs
//
// Test-double gateway: stands in for the real payment provider so the sample
// runs without external dependencies. Selected by configuration
// (ExternalGateway:UseStub, default true). Production sets UseStub=false and
// a real BaseUrl, which registers ExternalGatewayClient with the Polly
// pipeline instead — same Ports.IPaymentGateway contract, no consumer changes.
//
// Behaviour is token-driven and fully deterministic:
//   tok_timeout* → hangs 10 s, exceeding the processor's 4 s budget,
//                  which exercises the typed-timeout (503) path;
//   tok_decline* → immediate decline, which exercises the 400 path;
//   anything else → immediate success with a stub transaction id.
//
// Architecture note (S05): the stub is honest about what it is. It proves
// processor logic, not provider behaviour. Provider behaviour (429s, breaker
// trips, real latency) is the Polly pipeline's story and is verified against
// the real provider, never against this stub.

using System.Threading;
using PaymentService.Application.Ports;
using PaymentService.Core.Domain;

namespace PaymentService.Infrastructure.Gateway;

public sealed class StubGatewayClient : IPaymentGateway
{
    private int _charges;

    public async Task<GatewayResponse> ChargeAsync(
        Money amount, PaymentMethod method, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(method);

        if (method.Token.StartsWith("tok_timeout", StringComparison.Ordinal))
        {
            // Longer than the processor's 4 s gateway budget: the linked
            // timeout token fires first and the processor returns the typed
            // GatewayTimeout result. Never reaches the line below in practice.
            await Task.Delay(TimeSpan.FromSeconds(10), ct);
            return new GatewayResponse(true, "tx_stub_late", null);
        }

        if (method.Token.StartsWith("tok_decline", StringComparison.Ordinal))
            return new GatewayResponse(false, "", "CARD_DECLINED");

        var sequence = Interlocked.Increment(ref _charges);
        return new GatewayResponse(true, $"tx_stub_{sequence:D4}", null);
    }
}
