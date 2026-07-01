using Lumen.Application.Payments.Dtos;

namespace Lumen.Application.Payments;

/// <summary>
/// Abstraction for payment gateways. Implement per provider (dummy, Stripe, etc.).
/// </summary>
public interface IPaymentProvider
{
    string Name { get; }

    Task<PaymentResult> ProcessPaymentAsync(
        PaymentContext context,
        PaymentRequest request,
        CancellationToken cancellationToken = default);

    Task<PaymentResult> CaptureAsync(
        string transactionId,
        CancellationToken cancellationToken = default);

    Task<PaymentResult> RefundAsync(
        string transactionId,
        decimal amount,
        CancellationToken cancellationToken = default);
}