// book/samples/Chapter-01/DistributedPaymentService/src/PaymentService.Infrastructure/Services/InMemoryUserPreferenceService.cs
using System.Collections.Concurrent;
using PaymentService.Application.Ports;

namespace PaymentService.Infrastructure.Services;

public sealed class InMemoryUserPreferenceService : IUserPreferenceService
{
    private readonly ConcurrentDictionary<string, UserPreferences> _store = new();

    public Task<UserPreferences> GetAsync(string userId, CancellationToken ct)
    {
        var prefs = _store.GetOrAdd(userId, UserPreferences.Default);
        return Task.FromResult(prefs);
    }

    public Task SaveAsync(UserPreferences preferences, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(preferences);

        _store[preferences.UserId] = preferences;
        return Task.CompletedTask;
    }
}
