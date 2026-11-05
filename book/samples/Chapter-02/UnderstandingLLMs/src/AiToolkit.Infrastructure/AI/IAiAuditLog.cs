// S06 concept: structured, queryable record of AI-generated code
// acceptance. Persisted to allow post-incident analysis of which
// AI-generated components were involved in a failure and at what
// verification level they were accepted (Section 06).

namespace AiToolkit.Infrastructure.AI;

/// <summary>
/// Interface for an audit log that records AI-generated components accepted into the system, along with their verification levels and other relevant metadata.
/// This allows for post-incident analysis of AI-generated components and their associated risks.
/// </summary>
public interface IAiAuditLog {
    /// <summary>
    /// Complete history of all AI-generated components accepted into the
    /// </summary>
    IReadOnlyList<AiAuditEntry> Entries { get; }

    /// <summary>
    /// Find all entries with a verification level below the specified threshold.
    /// </summary>
    /// <param name="threshold">The verification level threshold.</param>
    /// <returns>A list of audit entries below the specified threshold.</returns>
    IReadOnlyList<AiAuditEntry> FindBelowLevel(VerificationLevel threshold);

    /// <summary>
    /// Record a new audit entry for an AI-generated component that has been accepted into the system.
    /// </summary>
    /// <param name="entry">The audit entry to record.</param>
    void Record(AiAuditEntry entry);
}
