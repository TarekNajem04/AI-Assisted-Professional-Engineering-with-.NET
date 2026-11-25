// book/samples/Chapter-03/EngineeringCommunicationToolkit/src/AiToolkit.Infrastructure/AI/ContextBuilder.cs
// S02: token-budget-aware context assembly with edge-loading.
using AiToolkit.Core.Tokens;
namespace AiToolkit.Infrastructure.AI;

public sealed record ContextBlock(string Content, int Priority, string SourceLabel);

public sealed class ContextBuilder
{
    private readonly List<ContextBlock> _blocks = [];
    private readonly int _tokenBudget;
    public ContextBuilder(int tokenBudget)
    {
        if (tokenBudget <= 0) throw new ArgumentOutOfRangeException(nameof(tokenBudget));
        _tokenBudget = tokenBudget;
    }
    public ContextBuilder Add(string content, int priority, string sourceLabel)
    {
        if (!string.IsNullOrWhiteSpace(content))
            _blocks.Add(new ContextBlock(content, priority, sourceLabel));
        return this;
    }
    public ContextAssemblyResult Build()
    {
        var ranked = _blocks.OrderByDescending(b => b.Priority).ToList();
        var included = new List<ContextBlock>(); var excluded = new List<ContextBlock>(); var used = 0;
        foreach (var b in ranked)
        {
            var t = TokenEstimator.EstimateProseTokens(b.Content);
            if (used + t <= _tokenBudget) { included.Add(b); used += t; } else excluded.Add(b);
        }
        var mid = (included.Count + 1) / 2;
        var edge = included.Take(mid).Concat(included.Skip(mid).Reverse()).ToList();
        var assembled = string.Join("\n\n---\n\n", edge.Select(b => $"[{b.SourceLabel}]\n{b.Content}"));
        return new ContextAssemblyResult(assembled, edge, excluded, used, _tokenBudget);
    }
}
public sealed record ContextAssemblyResult(
    string AssembledContext, IReadOnlyList<ContextBlock> IncludedBlocks,
    IReadOnlyList<ContextBlock> ExcludedBlocks, int UsedTokens, int BudgetTokens);
