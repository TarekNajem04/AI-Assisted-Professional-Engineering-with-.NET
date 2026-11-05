// book/samples/Chapter-02/UnderstandingLLMs/tests/Unit/AI/DeterministicAiBoundaryTests.cs
//
// Unit tests for S05's DeterministicAiBoundary<T>. Demonstrates exactly
// the architectural point Section 05 makes: everything outside the
// boundary is deterministic and testable with a stub IChatCompletionClient
// — no real, non-deterministic AI call is needed to verify this logic.

using AiToolkit.Application.Ports;
using AiToolkit.Infrastructure.AI;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Jeninnet.Assertions;

namespace AiToolkit.Tests.Unit.AI;

[TestClass]
public sealed class DeterministicAiBoundaryTests {
    [TestMethod]
    public async Task ExecuteAsync_ValidFirstResponse_ReturnsSuccessImmediatelyAsync() {
        var chatClient = new StubChatClient(responses: ["valid output"]);
        var parser = new StubParser(succeedsOn: 1);
        var boundary = CreateBoundary(chatClient, parser);

        var result = await boundary.ExecuteAsync("system", "user", default);

        (result.IsSuccess).Should().BeTrue();
        chatClient.CallCount.Should().Be(1);
    }

    [TestMethod]
    public async Task ExecuteAsync_FailsFirstSucceedsSecond_RetriesWithFeedbackAsync() {
        var chatClient = new StubChatClient(responses: ["bad output", "good output"]);
        var parser = new StubParser(succeedsOn: 2);
        var boundary = CreateBoundary(chatClient, parser);

        var result = await boundary.ExecuteAsync("system", "user", default);

        (result.IsSuccess).Should().BeTrue();
        chatClient.CallCount.Should().Be(2);

        // Verify the retry prompt included feedback about the failure —
        // this is the "retry with prompt adjustment" mechanism from S05.
        chatClient.LastUserPrompt.Should().Contain("IMPORTANT");
    }

    [TestMethod]
    public async Task ExecuteAsync_AllAttemptsFail_ReturnsTypedFailureNotExceptionAsync() {
        var chatClient = new StubChatClient(responses: ["bad", "bad", "bad"]);
        var parser = new StubParser(succeedsOn: int.MaxValue); // never succeeds
        var boundary = CreateBoundary(chatClient, parser);

        var result = await boundary.ExecuteAsync("system", "user", default);

        (result.IsSuccess).Should().BeFalse();
        result.Error.Should().NotBeNull();
        chatClient.CallCount.Should().Be(3); // MaxAttempts = 3
    }

    [TestMethod]
    public async Task ExecuteAsync_NeverCallsRealAiClient_StubIsSufficientForFullCoverageAsync() {
        // This test exists to make the architectural point explicit:
        // ALL of DeterministicAiBoundary's logic — retry, feedback,
        // success/failure typing — is fully covered using stubs.
        // No network call, no non-determinism, no flaky test.
        var chatClient = new StubChatClient(responses: ["x"]);
        var parser = new StubParser(succeedsOn: 1);
        var boundary = CreateBoundary(chatClient, parser);

        await boundary.ExecuteAsync("s", "u", default);
        await boundary.ExecuteAsync("s", "u", default);

        // Deterministic: same stub behaviour every time.
        chatClient.CallCount.Should().Be(2);
    }

    private static DeterministicAiBoundary<string> CreateBoundary(
        StubChatClient chatClient, StubParser parser) =>
        new(chatClient, parser, NullLogger<DeterministicAiBoundary<string>>.Instance);

    // ── Test doubles ─────────────────────────────────────────────────────

    private sealed class StubChatClient(string[] responses) : IChatCompletionClient {
        private int _index;
        public int CallCount { get; private set; }
        public string LastUserPrompt { get; private set; } = string.Empty;

        public Task<string> CompleteAsync(
            string systemPrompt, string userPrompt, CancellationToken ct) {
            CallCount++;
            LastUserPrompt = userPrompt;
            var response = responses[Math.Min(_index, responses.Length - 1)];
            _index++;
            return Task.FromResult(response);
        }

        public async IAsyncEnumerable<string> CompleteStreamingAsync(
            string systemPrompt, string userPrompt,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct) {
            yield return await CompleteAsync(systemPrompt, userPrompt, ct);
        }
    }

    private sealed class StubParser(int succeedsOn) : IAiOutputParser<string> {
        private int _attempt;

        public ParseResult<string> Parse(string raw) {
            _attempt++;
            return _attempt >= succeedsOn
                ? ParseResult<string>.Success(raw)
                : ParseResult<string>.Failure($"attempt {_attempt} not valid yet");
        }
    }
}
