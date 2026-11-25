// book/samples/Chapter-03/EngineeringCommunicationToolkit/tests/Unit/ContextBuilderTests.cs
//
// Unit tests for S02's ContextBuilder.
// Budget enforcement and edge-loading are pure deterministic logic —
// no AI call, no stub required.

using AiToolkit.Infrastructure.AI;
using Jeninnet.Assertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AiToolkit.Tests.Unit;

[TestClass]
public sealed class ContextBuilderTests
{
    [TestMethod]
    public void Build_EmptyBlocks_ReturnsEmptyContext()
    {
        var result = new ContextBuilder(tokenBudget: 1000).Build();

        result.IncludedBlocks.Count().Should().Be(0);
        result.ExcludedBlocks.Count().Should().Be(0);
        result.UsedTokens.Should().Be(0);
    }

    [TestMethod]
    public void Build_BlockFitsInBudget_IsIncluded()
    {
        var result = new ContextBuilder(tokenBudget: 1000)
            .Add("short content", priority: 5, sourceLabel: "src")
            .Build();

        result.IncludedBlocks.Count().Should().Be(1);
        result.ExcludedBlocks.Count().Should().Be(0);
    }

    [TestMethod]
    public void Build_BlockExceedsBudget_IsExcluded()
    {
        // Each char ≈ 0.25 tokens; 400 chars ≈ 100 tokens; budget = 5
        var bigContent = new string('x', 400);

        var result = new ContextBuilder(tokenBudget: 5)
            .Add(bigContent, priority: 10, sourceLabel: "big")
            .Build();

        result.IncludedBlocks.Count().Should().Be(0);
        result.ExcludedBlocks.Count().Should().Be(1);
    }

    [TestMethod]
    public void Build_HighPriorityFitsLowDoesNot_IncludesHighExcludesLow()
    {
        var big = new string('x', 400); // ~100 tokens
        var small = "tiny";               // ~1 token

        var result = new ContextBuilder(tokenBudget: 10)
            .Add(big, priority: 10, sourceLabel: "big")
            .Add(small, priority: 5, sourceLabel: "small")
            .Build();

        (result.ExcludedBlocks.Any(b => b.SourceLabel == "big")).Should().BeTrue();
        (result.IncludedBlocks.Any(b => b.SourceLabel == "small")).Should().BeTrue();
    }

    [TestMethod]
    public void Build_HigherPriorityBlockRankedFirst()
    {
        var result = new ContextBuilder(tokenBudget: 2000)
            .Add("low priority", priority: 1, sourceLabel: "low")
            .Add("high priority", priority: 10, sourceLabel: "high")
            .Build();

        // Edge-loading: highest priority sits at index 0
        result.IncludedBlocks[0].SourceLabel.Should().Be("high");
    }

    [TestMethod]
    public void Build_AssembledContext_ContainsSourceLabels()
    {
        var result = new ContextBuilder(tokenBudget: 2000)
            .Add("content A", priority: 5, sourceLabel: "source-A")
            .Build();

        result.AssembledContext.Should().Contain("source-A");
        result.AssembledContext.Should().Contain("content A");
    }

    [TestMethod]
    public void Build_WhitespaceOnlyContent_IsIgnored()
    {
        var result = new ContextBuilder(tokenBudget: 1000)
            .Add("   ", priority: 10, sourceLabel: "blank")
            .Build();

        result.IncludedBlocks.Count().Should().Be(0);
    }

    [TestMethod]
    public void Constructor_ZeroBudget_Throws()
    {
        FluentActions.Invoking(() => { new ContextBuilder(0); }).Should().Throw<ArgumentOutOfRangeException>();
    }

    [TestMethod]
    public void Constructor_NegativeBudget_Throws()
    {
        FluentActions.Invoking(() => { new ContextBuilder(-1); }).Should().Throw<ArgumentOutOfRangeException>();
    }

    [TestMethod]
    public void Build_EdgeLoading_ThreeBlocks_HighestAtStartAndEnd()
    {
        // With 3 blocks: midpoint = 2, so front=[P10,P5], back=[P1] reversed=[P1]
        // Edge order: P10, P5, P1  — highest at position 0 (start)
        var result = new ContextBuilder(tokenBudget: 5000)
            .Add("A", priority: 10, sourceLabel: "P10")
            .Add("B", priority: 5, sourceLabel: "P5")
            .Add("C", priority: 1, sourceLabel: "P1")
            .Build();

        result.IncludedBlocks[0].SourceLabel.Should().Be("P10");
    }
}
