// book/samples/Chapter-01/DistributedPaymentService/src/PaymentService.Application/Ports/IPaymentRepository.cs
using PaymentService.Core.Domain;

namespace PaymentService.Application.Ports;

public interface IPaymentRepository
{
    /// <summary>
    /// Atomically persists the payment record together with its idempotency
    /// key — one indivisible write, never two. This is the S01 fix: the
    /// original code wrote the record and the key in separate steps, so a
    /// crash between them caused double charges on retry. Adapters must
    /// honour the atomicity (DB transaction in production; single lock here).
    /// Residual window, documented not hidden: two CONCURRENT duplicate
    /// requests can both pass the existence check before either saves.
    /// Production closes it with a unique constraint on the idempotency key.
    /// </summary>
    Task SaveAsync(Payment payment, string idempotencyKey, CancellationToken ct);
    Task<bool> IdempotencyKeyExistsAsync(string key, CancellationToken ct);
    Task<Payment?> FindByIdAsync(PaymentId id, CancellationToken ct);
}
