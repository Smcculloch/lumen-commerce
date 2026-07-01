using Lumen.Application.Payments;
using Lumen.Application.Payments.Dtos;
using Lumen.Domain.Enums;
using Microsoft.Extensions.Options;

namespace Lumen.Infrastructure.Payments;

/// <summary>
/// Simulated payment provider for development and testing. Replace with Stripe, etc. in production.
/// </summary>
public sealed class DummyPaymentProvider : IPaymentProvider
{
    public const string ProviderName = "dummy";

    private readonly DummyPaymentOptions _options;

    public DummyPaymentProvider(IOptions<PaymentOptions> options)
    {
        _options = options.Value.Dummy;
    }

    public string Name => ProviderName;

    public Task<PaymentResult> ProcessPaymentAsync(
        PaymentContext context,
        PaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        var shouldFail = request.SimulateFailure || _options.SimulateFailure;
        if (!shouldFail && context.CustomerEmail.Contains("fail@", StringComparison.OrdinalIgnoreCase))
        {
            shouldFail = true;
        }

        if (shouldFail)
        {
            return Task.FromResult(new PaymentResult(
                Success: false,
                Status: PaymentStatus.Failed,
                TransactionId: null,
                Message: _options.FailureMessage));
        }

        var transactionId = $"DUMMY-{Guid.NewGuid():N}".ToUpperInvariant();
        return Task.FromResult(new PaymentResult(
            Success: true,
            Status: PaymentStatus.Authorized,
            TransactionId: transactionId,
            Message: "Payment authorized (dummy provider)."));
    }

    public Task<PaymentResult> CaptureAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
        {
            return Task.FromResult(new PaymentResult(
                Success: false,
                Status: PaymentStatus.Failed,
                TransactionId: transactionId,
                Message: "Transaction id is required."));
        }

        return Task.FromResult(new PaymentResult(
            Success: true,
            Status: PaymentStatus.Captured,
            TransactionId: transactionId,
            Message: "Payment captured (dummy provider)."));
    }

    public Task<PaymentResult> RefundAsync(
        string transactionId,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
        {
            return Task.FromResult(new PaymentResult(
                Success: false,
                Status: PaymentStatus.Failed,
                TransactionId: transactionId,
                Message: "Transaction id is required."));
        }

        if (amount <= 0)
        {
            return Task.FromResult(new PaymentResult(
                Success: false,
                Status: PaymentStatus.Failed,
                TransactionId: transactionId,
                Message: "Refund amount must be greater than zero."));
        }

        return Task.FromResult(new PaymentResult(
            Success: true,
            Status: PaymentStatus.Refunded,
            TransactionId: transactionId,
            Message: $"Refunded {amount:C} (dummy provider)."));
    }
}