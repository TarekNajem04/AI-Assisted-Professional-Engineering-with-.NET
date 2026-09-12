// book/samples/Chapter-01/DistributedPaymentService/src/PaymentService.Core/Domain/Payment.cs
// Domain model with explicit invariants.
// No framework dependencies — plain records and value objects.

namespace PaymentService.Core.Domain;

/// <summary>
/// Payment aggregate root.
/// Encapsulates all invariants — nothing leaves this class in an invalid state.
/// Architecture note (S03): domain model has no knowledge of infrastructure.
/// Any persistence, caching, or messaging concern belongs in Infrastructure.
/// </summary>
public sealed class Payment
{
    public PaymentId     Id           { get; private init; }
    public CustomerId    CustomerId   { get; private init; }
    public Money         Amount       { get; private init; }
    public PaymentMethod Method       { get; private init; } = default!;
    public PaymentStatus Status       { get; private set; }
    public DateTimeOffset CreatedAt   { get; private init; }
    public DateTimeOffset? ProcessedAt { get; private set; }

    private Payment() { }   // EF Core

    public static Payment Create(
        CustomerId customerId,
        Money amount,
        PaymentMethod method)
    {
        ArgumentNullException.ThrowIfNull(customerId);
        ArgumentNullException.ThrowIfNull(amount);
        ArgumentNullException.ThrowIfNull(method);

        if (amount.Value <= 0)
            throw new DomainException("Payment amount must be positive.");

        return new Payment
        {
            Id         = PaymentId.New(),
            CustomerId = customerId,
            Amount     = amount,
            Method     = method,
            Status     = PaymentStatus.Pending,
            CreatedAt  = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>
    /// Marks the payment as successfully processed by the gateway.
    /// Architecture note (S03): The domain enforces the state transition.
    /// The infrastructure layer cannot bypass it by setting Status directly.
    /// </summary>
    public void MarkProcessed()
    {
        if (Status is not PaymentStatus.Pending)
            throw new DomainException(
                $"Cannot process payment in status {Status}.");

        Status      = PaymentStatus.Processed;
        ProcessedAt = DateTimeOffset.UtcNow;
    }

    public void MarkFailed()
    {
        if (Status is not PaymentStatus.Pending)
            throw new DomainException(
                $"Cannot fail payment in status {Status}.");

        Status = PaymentStatus.Failed;
    }
}

// ── Value objects ─────────────────────────────────────────────────────────

/// <summary>
/// Strongly-typed identifier prevents accidental mix-up of IDs.
/// Architecture note (S03): type safety is cheaper than runtime validation.
/// </summary>
public readonly record struct PaymentId(Guid Value)
{
    public static PaymentId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}

public readonly record struct CustomerId(Guid Value)
{
    public static CustomerId New() => new(Guid.NewGuid());
}

public readonly record struct Money(decimal Value, string Currency)
{
    public static Money Of(decimal value, string currency = "USD")
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException("Currency cannot be empty.");
        return new(value, currency.ToUpperInvariant());
    }
}

public sealed record PaymentMethod(string Type, string Token)
{
    public static PaymentMethod Card(string token) => new("card", token);
    public static PaymentMethod BankTransfer(string token) => new("bank_transfer", token);
}

public enum PaymentStatus { Pending, Processed, Failed }

public sealed class DomainException(string message) : Exception(message);
