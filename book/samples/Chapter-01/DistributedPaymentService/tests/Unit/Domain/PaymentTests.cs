// book/samples/Chapter-01/DistributedPaymentService/tests/Unit/Domain/PaymentTests.cs
//
// Unit tests for the Payment domain model.
// Tests specification correctness — behavior against defined contracts.
// Architecture note (S02): these tests do NOT catch the integration failure
// modes described in code-01-02. That is what Integration tests are for.

using PaymentService.Core.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

        Assert.AreEqual(PaymentStatus.Pending, payment.Status);
        Assert.AreEqual(customerId, payment.CustomerId);
        Assert.AreEqual(amount, payment.Amount);
    }

    [TestMethod]
    public void Create_ZeroAmount_ThrowsDomainException()
    {
        var ex = Assert.ThrowsException<DomainException>(() =>
            Payment.Create(CustomerId.New(), Money.Of(0m), PaymentMethod.Card("tok")));

        StringAssert.Contains(ex.Message, "positive");
    }

    [TestMethod]
    public void MarkProcessed_FromPending_TransitionsToProcessed()
    {
        var payment = Payment.Create(
            CustomerId.New(), Money.Of(50m), PaymentMethod.Card("tok_test"));

        payment.MarkProcessed();

        Assert.AreEqual(PaymentStatus.Processed, payment.Status);
        Assert.IsNotNull(payment.ProcessedAt);
    }

    [TestMethod]
    public void MarkProcessed_AlreadyProcessed_ThrowsDomainException()
    {
        var payment = Payment.Create(
            CustomerId.New(), Money.Of(50m), PaymentMethod.Card("tok_test"));
        payment.MarkProcessed();

        Assert.ThrowsException<DomainException>(() => payment.MarkProcessed());
    }

    [TestMethod]
    public void MarkFailed_FromPending_TransitionsToFailed()
    {
        var payment = Payment.Create(
            CustomerId.New(), Money.Of(75m), PaymentMethod.BankTransfer("iban_test"));

        payment.MarkFailed();

        Assert.AreEqual(PaymentStatus.Failed, payment.Status);
    }

    [TestMethod]
    public void MarkFailed_AlreadyFailed_ThrowsDomainException()
    {
        var payment = Payment.Create(
            CustomerId.New(), Money.Of(75m), PaymentMethod.BankTransfer("iban_test"));
        payment.MarkFailed();

        Assert.ThrowsException<DomainException>(() => payment.MarkFailed());
    }

    // ── Money value object ───────────────────────────────────────────────

    [TestMethod]
    public void MoneyOf_NormalisesUpperCase()
    {
        var money = Money.Of(10m, "usd");
        Assert.AreEqual("USD", money.Currency);
    }

    [TestMethod]
    public void MoneyOf_EmptyCurrency_ThrowsDomainException()
    {
        Assert.ThrowsException<DomainException>(() => Money.Of(10m, ""));
    }

    // ── PaymentId value object ───────────────────────────────────────────

    [TestMethod]
    public void PaymentId_New_GeneratesUniqueIds()
    {
        var id1 = PaymentId.New();
        var id2 = PaymentId.New();
        Assert.AreNotEqual(id1, id2);
    }
}
