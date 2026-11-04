// book/samples/Chapter-01/DistributedPaymentService/tests/Unit/Domain/PaymentTests.cs
//
// Unit tests for the Payment domain model.
// Tests specification correctness — behavior against defined contracts.
// Architecture note (S02): these tests do NOT catch the integration failure
// modes described in code-01-02. That is what Integration tests are for.

using PaymentService.Core.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Jeninnet.Assertions;

namespace PaymentService.Tests.Unit.Domain;

[TestClass]
public sealed class PaymentTests
{
    [TestMethod]
    public void Create_ValidArguments_CreatesPaymentInPendingStatus()
    {
        var customerId = CustomerId.New();
        var amount     = Money.Of(100m, "USD");
        var method     = PaymentMethod.Card("tok_test");

        var payment = Payment.Create(customerId, amount, method);

        payment.Status.Should().Be(PaymentStatus.Pending);
        payment.CustomerId.Should().Be(customerId);
        payment.Amount.Should().Be(amount);
    }

    [TestMethod]
    public void Create_ZeroAmount_ThrowsDomainException()
    {
        var ex = FluentActions.Invoking(() => { Payment.Create(CustomerId.New(), Money.Of(0m), PaymentMethod.Card("tok")); }).Should().Throw<DomainException>().Which;

        ex.Message.Should().Contain("positive");
    }

    [TestMethod]
    public void MarkProcessed_FromPending_TransitionsToProcessed()
    {
        var payment = Payment.Create(
            CustomerId.New(), Money.Of(50m), PaymentMethod.Card("tok_test"));

        payment.MarkProcessed();

        payment.Status.Should().Be(PaymentStatus.Processed);
        payment.ProcessedAt.Should().HaveValue();
    }

    [TestMethod]
    public void MarkProcessed_AlreadyProcessed_ThrowsDomainException()
    {
        var payment = Payment.Create(
            CustomerId.New(), Money.Of(50m), PaymentMethod.Card("tok_test"));
        payment.MarkProcessed();

        FluentActions.Invoking(() => { payment.MarkProcessed(); }).Should().Throw<DomainException>();
    }

    [TestMethod]
    public void MarkFailed_FromPending_TransitionsToFailed()
    {
        var payment = Payment.Create(
            CustomerId.New(), Money.Of(75m), PaymentMethod.BankTransfer("iban_test"));

        payment.MarkFailed();

        payment.Status.Should().Be(PaymentStatus.Failed);
    }

    [TestMethod]
    public void MarkFailed_AlreadyFailed_ThrowsDomainException()
    {
        var payment = Payment.Create(
            CustomerId.New(), Money.Of(75m), PaymentMethod.BankTransfer("iban_test"));
        payment.MarkFailed();

        FluentActions.Invoking(() => { payment.MarkFailed(); }).Should().Throw<DomainException>();
    }

    // ── Money value object ───────────────────────────────────────────────

    [TestMethod]
    public void MoneyOf_NormalisesUpperCase()
    {
        var money = Money.Of(10m, "usd");
        money.Currency.Should().Be("USD");
    }

    [TestMethod]
    public void MoneyOf_EmptyCurrency_ThrowsDomainException()
    {
        FluentActions.Invoking(() => { Money.Of(10m, ""); }).Should().Throw<DomainException>();
    }

    // ── PaymentId value object ───────────────────────────────────────────

    [TestMethod]
    public void PaymentId_New_GeneratesUniqueIds()
    {
        var id1 = PaymentId.New();
        var id2 = PaymentId.New();
        id2.Should().NotBe(id1);
    }
}
