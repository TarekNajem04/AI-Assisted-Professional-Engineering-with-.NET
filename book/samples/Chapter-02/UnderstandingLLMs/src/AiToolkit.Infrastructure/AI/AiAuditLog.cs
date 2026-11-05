// S06 concept: structured, queryable record of AI-generated code
// acceptance. Persisted to allow post-incident analysis of which
// AI-generated components were involved in a failure and at what
// verification level they were accepted (Section 06).

using Microsoft.Extensions.Logging;

namespace AiToolkit.Infrastructure.AI;

public sealed record AiAuditEntry(
    string ComponentId,
    VerificationLevel VerificationLevel,
    string Notes,
    DateTimeOffset AcceptedAt,
    string AcceptedBy);

/// <summary>
/// Audit trail for AI-generated component acceptance.
/// In this sample, entries are held in memory; a production implementation would persist
/// to a database or structured log sink for durable post-incident queries.
/// </summary>
/// <param name="logger">The logger to use for logging audit events.</param>
public sealed class AiAuditLog(ILogger<AiAuditLog> logger) : IAiAuditLog {
    private readonly List<AiAuditEntry> _entries = [];

    // NOTE: System.Threading.Lock requires C# 13 / .NET 9. This project
    // targets net8.0, so a plain object monitor is used instead — the
    // standard, fully-portable .NET 8 synchronization primitive.
    private readonly object _lock = new();

    /// <inheritdoc/>
    public void Record(AiAuditEntry entry) {
        ArgumentNullException.ThrowIfNull(entry);

        lock(_lock) {
            _entries.Add(entry);
        }

        logger.LogInformation(
            "AI-generated component accepted: {ComponentId} " +
            "at verification level {Level} by {AcceptedBy}. Notes: {Notes}",
            entry.ComponentId,
            entry.VerificationLevel,
            entry.AcceptedBy,
            entry.Notes);
    }

    /// <inheritdoc/>
    public IReadOnlyList<AiAuditEntry> Entries {
        get { lock(_lock) { return _entries.ToList().AsReadOnly(); } }
    }

    /// <inheritdoc/>
    public IReadOnlyList<AiAuditEntry> FindBelowLevel(VerificationLevel threshold) {
        lock(_lock) {
            return _entries
                .Where(e => e.VerificationLevel < threshold)
                .ToList()
                .AsReadOnly();
        }
    }
}
