// S04 concept: validates AI-generated .NET code suggestions before they
// enter a build. Catches fabricated/moved namespaces and outdated-pattern
// signatures — the "API/package fabrication" and "plausible-but-outdated"
// categories from Section 04's failure-mode taxonomy. This is NOT a
// complete solution to hallucination (Section 04 is explicit about this),
// it is the architectural pattern of an explicit deterministic check
// applied to probabilistic output before that output is trusted.

using System.Collections.Frozen;

namespace AiToolkit.Infrastructure.AI;

public class AiSuggestionValidator : IAiSuggestionValidator {
    // Known-bad namespace prefixes that appear in AI suggestions but
    // do not correspond to current .NET APIs (moved, renamed, internal).
    private static readonly FrozenSet<string> _knownFabricatedNamespaces =
        FrozenSet.ToFrozenSet(
            new[]
            {
                "Microsoft.AspNetCore.Http.Extensions",
                "System.Web.Http",
                "Microsoft.Extensions.Logging.Console.Internal",
            },
            StringComparer.Ordinal
        );

    // Method/type signatures that are plausible-but-outdated.
    private static readonly FrozenSet<string> _outdatedPatternSignatures =
        FrozenSet.ToFrozenSet(
            new[]
            {
                ".Download(",
                ".Result",
                "Thread.Sleep(",
                "HttpWebRequest",
                "ServiceStack",
            },
            StringComparer.Ordinal
        );

    public ValidationResult Validate(string suggestedCode) {
        ArgumentNullException.ThrowIfNull(suggestedCode);

        var issues = new List<ValidationIssue>();

        foreach(var ns in _knownFabricatedNamespaces.Where(n =>
                     suggestedCode.Contains(n, StringComparison.Ordinal))) {
            issues.Add(new ValidationIssue(
                Severity.Error,
                "AI001",
                $"Fabricated or moved namespace detected: '{ns}'. Verify the correct current namespace before using."));
        }

        foreach(var pattern in _outdatedPatternSignatures.Where(p =>
                     suggestedCode.Contains(p, StringComparison.Ordinal))) {
            issues.Add(new ValidationIssue(
                Severity.Warning,
                "AI002",
                $"Outdated or unsafe pattern detected: '{pattern}'. Review for modern .NET equivalents."));
        }

        return issues.Count == 0
            ? ValidationResult.Valid
            : new ValidationResult(false, issues);
    }
}

// ── Value types ───────────────────────────────────────────────────────────

public enum Severity { Warning, Error }

public sealed record ValidationIssue(
    Severity Severity,
    string Code,
    string Message
);

public sealed record ValidationResult(
    bool IsValid,
    IReadOnlyList<ValidationIssue>? Issues = null
) {
    public static readonly ValidationResult Valid = new(true);
}
