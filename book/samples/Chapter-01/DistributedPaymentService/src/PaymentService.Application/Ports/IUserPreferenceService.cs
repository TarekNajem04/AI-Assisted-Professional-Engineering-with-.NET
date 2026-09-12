// book/samples/Chapter-01/DistributedPaymentService/src/PaymentService.Application/Ports/IUserPreferenceService.cs
namespace PaymentService.Application.Ports;

public interface IUserPreferenceService
{
    Task<UserPreferences> GetAsync(string userId, CancellationToken ct);
    Task SaveAsync(UserPreferences preferences, CancellationToken ct);
}

public sealed record UserPreferences(string UserId, string Currency, string Language, bool EmailNotifications)
{
    public static UserPreferences Default(string userId) => new(userId, "USD", "en", true);
}
