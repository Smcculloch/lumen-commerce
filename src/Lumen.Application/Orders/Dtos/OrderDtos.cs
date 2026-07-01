using Lumen.Domain.Enums;

namespace Lumen.Application.Orders.Dtos;

public sealed record OrderAddressDto(
    string Name,
    string Line1,
    string? Line2,
    string City,
    string? Region,
    string PostalCode,
    string Country);

public sealed record OrderLineItemDto(
    Guid Id,
    Guid ProductId,
    Guid? ProductVariantId,
    string Sku,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);

public sealed record OrderDto(
    Guid Id,
    string OrderNumber,
    Guid? CustomerId,
    string CustomerName,
    string Email,
    OrderAddressDto ShippingAddress,
    OrderAddressDto BillingAddress,
    string? OrderNotes,
    OrderStatus Status,
    PaymentStatus PaymentStatus,
    string? PaymentProvider,
    string? PaymentTransactionId,
    string? PaymentMessage,
    decimal AmountCaptured,
    decimal AmountRefunded,
    decimal Subtotal,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<OrderLineItemDto> Items);

public sealed record PlaceOrderRequest(
    string CustomerName,
    string Email,
    OrderAddressDto ShippingAddress,
    OrderAddressDto BillingAddress,
    string? OrderNotes,
    Guid? CustomerId);

public sealed record CheckoutResult(
    bool PaymentSucceeded,
    OrderDto Order,
    string? PaymentError);

public sealed record OrderListFilter(
    OrderStatus? Status = null,
    PaymentStatus? PaymentStatus = null,
    Guid? CustomerId = null,
    string? Search = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null);

public sealed record OrderHistoryEntryDto(
    Guid Id,
    Guid OrderId,
    OrderHistoryEventType EventType,
    OrderStatus? PreviousStatus,
    OrderStatus? NewStatus,
    string Message,
    string? Actor,
    DateTimeOffset CreatedAt);

public sealed record UpdateOrderStatusRequest(
    OrderStatus Status,
    string? Note = null,
    string? Actor = null);