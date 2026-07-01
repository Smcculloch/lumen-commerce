using Lumen.Domain.Enums;

namespace Lumen.Application.Payments.Dtos;

public sealed record PaymentContext(
    Guid OrderId,
    string OrderNumber,
    decimal Amount,
    string Currency,
    string CustomerEmail);

public sealed record PaymentRequest(
    string? CardholderName = null,
    bool SimulateFailure = false);

public sealed record PaymentResult(
    bool Success,
    PaymentStatus Status,
    string? TransactionId,
    string? Message);