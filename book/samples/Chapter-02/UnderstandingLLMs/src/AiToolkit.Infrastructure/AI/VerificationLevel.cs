// book/samples/Chapter-02/UnderstandingLLMs/src/AiToolkit.Infrastructure/AI/VerificationLevel.cs
//
// S06 concept: verification level applied to an AI-generated artifact
// before it was accepted into the codebase. Correlates to the calibrated
// trust matrix from Section 06: task type × training-distribution density.

namespace AiToolkit.Infrastructure.AI;

public enum VerificationLevel {
    /// <summary>
    /// Compilation + standard test suite only.
    /// Appropriate for: dense-distribution pattern tasks.
    /// </summary>
    Light = 1,

    /// <summary>
    /// Compilation + standard tests + explicit API surface verification
    /// + targeted tests for known failure modes.
    /// Appropriate for: sparse-distribution pattern tasks OR
    /// dense-distribution reasoning tasks.
    /// </summary>
    Moderate = 2,

    /// <summary>
    /// Independent logical verification + specific tests for the exact
    /// correctness property in question + architecture review.
    /// Appropriate for: reasoning tasks in any distribution region.
    /// </summary>
    Deep = 3,

    /// <summary>
    /// Formal verification or authoritative documentation cross-reference
    /// required. Appropriate for: sparse-distribution reasoning tasks
    /// (security, concurrency semantics, compliance).
    /// </summary>
    Formal = 4,
}
