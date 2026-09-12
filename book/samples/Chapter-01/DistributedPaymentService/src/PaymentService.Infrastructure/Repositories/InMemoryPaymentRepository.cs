// book/samples/Chapter-01/DistributedPaymentService/src/PaymentService.Infrastructure/Repositories/InMemoryPaymentRepository.cs
using System.Collections.Concurrent;
using PaymentService.Application.Ports;
using PaymentService.Core.Domain;

namespace PaymentService.Infrastructure.Repositories;

public sealed class InMemoryPaymentRepository : IPaymentRepository
{
    private readonly ConcurrentDictionary<Guid, Payment> _payments = new();
    private readonly ConcurrentDictionary<string, PaymentId> _idempotencyKeys = new();
    // One gate for BOTH dictionaries: the pair is written indivisibly,
    // honouring the atomicity the IPaymentRepository contract promises.
    // Production equivalent: a single database transaction.
    private readonly object _writeGate = new();

    public Task SaveAsync(Payment payment, string idempotencyKey, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(payment);
        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);

        lock (_writeGate)
        {
            _payments[payment.Id.Value] = payment;
            _idempotencyKeys[idempotencyKey] = payment.Id;
        }
        return Task.CompletedTask;
    }

    public Task<bool> IdempotencyKeyExistsAsync(string key, CancellationToken ct)
        => Task.FromResult(_idempotencyKeys.ContainsKey(key));

    public Task<Payment?> FindByIdAsync(PaymentId id, CancellationToken ct)
    {
        _payments.TryGetValue(id.Value, out var payment);
        return Task.FromResult(payment);
    }
}
