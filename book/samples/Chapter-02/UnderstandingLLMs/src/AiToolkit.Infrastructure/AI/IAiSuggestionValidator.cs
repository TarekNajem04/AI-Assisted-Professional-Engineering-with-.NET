// S04 concept: validates AI-generated .NET code suggestions before they
// enter a build. Catches fabricated/moved namespaces and outdated-pattern
// signatures — the "API/package fabrication" and "plausible-but-outdated"
// categories from Section 04's failure-mode taxonomy. This is NOT a
// complete solution to hallucination (Section 04 is explicit about this),
// it is the architectural pattern of an explicit deterministic check
// applied to probabilistic output before that output is trusted.

namespace AiToolkit.Infrastructure.AI;

public interface IAiSuggestionValidator {
    ValidationResult Validate(string suggestedCode);
}
