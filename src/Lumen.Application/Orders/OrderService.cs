using Lumen.Application.Cart.Dtos;
using Lumen.Application.Orders.Dtos;
using Lumen.Application.Payments;
using Lumen.Application.Payments.Dtos;
using Lumen.Domain.Enums;
using Lumen.Domain.Orders;
using Microsoft.Extensions.Options;

namespace Lumen.Application.Orders;

public sealed class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;
    private readonly IOrderHistoryRepository _historyRepository;
    private readonly IPaymentProviderResolver _paymentProviderResolver;
    private readonly PaymentOptions _paymentOptions;

    public OrderService(
        IOrderRepository repository,
        IOrderHistoryRepository historyRepository,
        IPaymentProviderResolver paymentProviderResolver,
        IOptions<PaymentOptions> paymentOptions)
    {
        _repository = repository;
        _historyRepository = historyRepository;
        _paymentProviderResolver = paymentProviderResolver;
        _paymentOptions = paymentOptions.Value;
    }

    public Task<OrderDto> PlaceOrderFromCartAsync(
        CartDto cart,
        PlaceOrderRequest request,
        CancellationToken cancellationToken = default) =>
        CreateOrderFromCartAsync(cart, request, cancellationToken);

    public async Task<CheckoutResult> CheckoutWithPaymentAsync(
        CartDto cart,
        PlaceOrderRequest placeRequest,
        PaymentRequest paymentRequest,
        CancellationToken cancellationToken = default)
    {
        var order = await CreateOrderEntityFromCartAsync(cart, placeRequest, cancellationToken);
        await _repository.AddAsync(order, cancellationToken);
        await RecordHistoryAsync(
            order.Id,
            OrderHistoryEventType.Created,
            $"Order {order.OrderNumber} created from checkout.",
            newStatus: order.Status,
            cancellationToken: cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        var provider = _paymentProviderResolver.ResolveActiveProvider();
        var context = new PaymentContext(
            order.Id,
            order.OrderNumber,
            order.Subtotal,
            _paymentOptions.Currency,
            order.Email);

        var paymentResult = await provider.ProcessPaymentAsync(context, paymentRequest, cancellationToken);

        if (paymentResult.Success)
        {
            order.ApplyPaymentOutcome(
                provider.Name,
                paymentResult.Status,
                paymentResult.TransactionId,
                paymentResult.Message,
                paymentSucceeded: true);

            await RecordHistoryAsync(
                order.Id,
                OrderHistoryEventType.PaymentEvent,
                paymentResult.Message ?? "Payment authorized.",
                newStatus: order.Status,
                cancellationToken: cancellationToken);
        }
        else
        {
            order.ApplyPaymentFailure(provider.Name, paymentResult.Message);

            await RecordHistoryAsync(
                order.Id,
                OrderHistoryEventType.PaymentEvent,
                paymentResult.Message ?? "Payment failed.",
                newStatus: order.Status,
                cancellationToken: cancellationToken);
        }

        await _repository.UpdateAsync(order, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return new CheckoutResult(
            paymentResult.Success,
            Map(order),
            paymentResult.Success ? null : paymentResult.Message);
    }

    public async Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _repository.GetByIdAsync(id, cancellationToken);
        return order is null ? null : Map(order);
    }

    public async Task<OrderDto?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        var order = await _repository.GetByOrderNumberAsync(orderNumber, cancellationToken);
        return order is null ? null : Map(order);
    }

    public async Task<IReadOnlyList<OrderDto>> ListAsync(OrderListFilter filter, CancellationToken cancellationToken = default)
    {
        var orders = await _repository.ListAsync(filter, cancellationToken);
        return orders.Select(Map).ToList();
    }

    public Task<OrderDto> UpdateStatusAsync(Guid id, OrderStatus status, CancellationToken cancellationToken = default) =>
        UpdateStatusWithNoteAsync(id, new UpdateOrderStatusRequest(status), cancellationToken);

    public async Task<OrderDto> UpdateStatusWithNoteAsync(
        Guid id,
        UpdateOrderStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var order = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Order was not found.");

        var previousStatus = order.Status;
        if (previousStatus == request.Status && string.IsNullOrWhiteSpace(request.Note))
        {
            return Map(order);
        }

        order.SetStatus(request.Status);
        await _repository.UpdateAsync(order, cancellationToken);

        var message = string.IsNullOrWhiteSpace(request.Note)
            ? $"Status changed from {previousStatus} to {request.Status}."
            : request.Note.Trim();

        await RecordHistoryAsync(
            order.Id,
            OrderHistoryEventType.StatusChanged,
            message,
            previousStatus,
            request.Status,
            request.Actor,
            cancellationToken);

        await _repository.SaveChangesAsync(cancellationToken);
        return Map(order);
    }

    public async Task<OrderDto> CancelOrderAsync(
        Guid id,
        string? reason = null,
        string? actor = null,
        CancellationToken cancellationToken = default)
    {
        var order = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Order was not found.");

        var previousStatus = order.Status;
        order.Cancel();
        await _repository.UpdateAsync(order, cancellationToken);

        var message = string.IsNullOrWhiteSpace(reason)
            ? "Order cancelled."
            : reason.Trim();

        await RecordHistoryAsync(
            order.Id,
            OrderHistoryEventType.Cancelled,
            message,
            previousStatus,
            OrderStatus.Cancelled,
            actor,
            cancellationToken);

        await _repository.SaveChangesAsync(cancellationToken);
        return Map(order);
    }

    public async Task<OrderDto> SendOrderNotificationAsync(
        Guid id,
        string? message = null,
        string? actor = null,
        CancellationToken cancellationToken = default)
    {
        var order = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Order was not found.");

        var notificationMessage = string.IsNullOrWhiteSpace(message)
            ? $"Order status notification queued for {order.Email} (stub — no email provider configured)."
            : message.Trim();

        await RecordHistoryAsync(
            order.Id,
            OrderHistoryEventType.Notification,
            notificationMessage,
            actor: actor,
            cancellationToken: cancellationToken);

        await _historyRepository.SaveChangesAsync(cancellationToken);
        return Map(order);
    }

    public async Task<IReadOnlyList<OrderHistoryEntryDto>> GetOrderHistoryAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var order = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Order was not found.");

        var history = await _historyRepository.ListByOrderIdAsync(id, cancellationToken);
        if (history.Count > 0)
        {
            return history.Select(MapHistory).ToList();
        }

        return
        [
            new OrderHistoryEntryDto(
                Guid.Empty,
                order.Id,
                OrderHistoryEventType.Created,
                null,
                order.Status,
                $"Order {order.OrderNumber} placed.",
                null,
                order.CreatedAt)
        ];
    }

    public async Task<OrderDto> CapturePaymentAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _repository.GetByIdAsync(orderId, cancellationToken)
            ?? throw new InvalidOperationException("Order was not found.");

        if (string.IsNullOrWhiteSpace(order.PaymentTransactionId))
        {
            throw new InvalidOperationException("Order has no payment transaction to capture.");
        }

        if (order.PaymentStatus is not PaymentStatus.Authorized and not PaymentStatus.Pending)
        {
            throw new InvalidOperationException($"Payment cannot be captured while status is {order.PaymentStatus}.");
        }

        var provider = ResolveProviderForOrder(order);
        var result = await provider.CaptureAsync(order.PaymentTransactionId, cancellationToken);

        if (!result.Success)
        {
            throw new InvalidOperationException(result.Message ?? "Payment capture failed.");
        }

        order.ApplyCapture(order.Subtotal, result.Status, result.Message);
        await _repository.UpdateAsync(order, cancellationToken);

        await RecordHistoryAsync(
            order.Id,
            OrderHistoryEventType.PaymentEvent,
            result.Message ?? "Payment captured.",
            cancellationToken: cancellationToken);

        await _repository.SaveChangesAsync(cancellationToken);
        return Map(order);
    }

    public async Task<OrderDto> RefundPaymentAsync(Guid orderId, decimal amount, CancellationToken cancellationToken = default)
    {
        var order = await _repository.GetByIdAsync(orderId, cancellationToken)
            ?? throw new InvalidOperationException("Order was not found.");

        if (string.IsNullOrWhiteSpace(order.PaymentTransactionId))
        {
            throw new InvalidOperationException("Order has no payment transaction to refund.");
        }

        if (amount <= 0)
        {
            throw new InvalidOperationException("Refund amount must be greater than zero.");
        }

        var provider = ResolveProviderForOrder(order);
        var result = await provider.RefundAsync(order.PaymentTransactionId, amount, cancellationToken);

        if (!result.Success)
        {
            throw new InvalidOperationException(result.Message ?? "Payment refund failed.");
        }

        var newStatus = order.AmountRefunded + amount >= order.Subtotal
            ? PaymentStatus.Refunded
            : PaymentStatus.PartiallyRefunded;

        order.ApplyRefund(amount, newStatus, result.Message);
        await _repository.UpdateAsync(order, cancellationToken);

        await RecordHistoryAsync(
            order.Id,
            OrderHistoryEventType.PaymentEvent,
            result.Message ?? $"Refunded {amount:C}.",
            cancellationToken: cancellationToken);

        await _repository.SaveChangesAsync(cancellationToken);
        return Map(order);
    }

    private async Task RecordHistoryAsync(
        Guid orderId,
        OrderHistoryEventType eventType,
        string message,
        OrderStatus? previousStatus = null,
        OrderStatus? newStatus = null,
        string? actor = null,
        CancellationToken cancellationToken = default)
    {
        var entry = OrderHistoryEntry.Create(
            orderId,
            eventType,
            message,
            previousStatus,
            newStatus,
            actor);

        await _historyRepository.AddAsync(entry, cancellationToken);
    }

    private IPaymentProvider ResolveProviderForOrder(Order order)
    {
        if (!string.IsNullOrWhiteSpace(order.PaymentProvider))
        {
            var match = _paymentProviderResolver.GetAllProviders()
                .FirstOrDefault(p => string.Equals(p.Name, order.PaymentProvider, StringComparison.OrdinalIgnoreCase));

            if (match is not null)
            {
                return match;
            }
        }

        return _paymentProviderResolver.ResolveActiveProvider();
    }

    private async Task<OrderDto> CreateOrderFromCartAsync(
        CartDto cart,
        PlaceOrderRequest request,
        CancellationToken cancellationToken)
    {
        var order = await CreateOrderEntityFromCartAsync(cart, request, cancellationToken);
        await _repository.AddAsync(order, cancellationToken);
        await RecordHistoryAsync(
            order.Id,
            OrderHistoryEventType.Created,
            $"Order {order.OrderNumber} created.",
            newStatus: order.Status,
            cancellationToken: cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return Map(order);
    }

    private async Task<Order> CreateOrderEntityFromCartAsync(
        CartDto cart,
        PlaceOrderRequest request,
        CancellationToken cancellationToken)
    {
        if (cart.Items.Count == 0)
        {
            throw new InvalidOperationException("Cannot place an order from an empty cart.");
        }

        var shipping = MapAddress(request.ShippingAddress);
        var billing = MapAddress(request.BillingAddress);

        var orderNumber = await GenerateOrderNumberAsync(cancellationToken);
        var lines = cart.Items
            .Select(i => new Order.OrderLineInput(
                i.ProductId,
                i.ProductVariantId,
                i.Sku,
                i.ProductName,
                i.Quantity,
                i.UnitPrice))
            .ToList();

        return Order.Create(
            orderNumber,
            request.CustomerId,
            request.CustomerName,
            request.Email,
            shipping,
            billing,
            request.OrderNotes,
            lines);
    }

    private async Task<string> GenerateOrderNumberAsync(CancellationToken cancellationToken)
    {
        var count = await _repository.CountAsync(cancellationToken);
        return $"LMN-{DateTime.UtcNow:yyyyMMdd}-{count + 1:D5}";
    }

    private static OrderAddress MapAddress(OrderAddressDto dto) =>
        OrderAddress.Create(
            dto.Name,
            dto.Line1,
            dto.Line2,
            dto.City,
            dto.Region,
            dto.PostalCode,
            dto.Country);

    private static OrderDto Map(Order order) =>
        new(
            order.Id,
            order.OrderNumber,
            order.CustomerId,
            order.CustomerName,
            order.Email,
            MapAddress(order.ShippingAddress),
            MapAddress(order.BillingAddress),
            order.OrderNotes,
            order.Status,
            order.PaymentStatus,
            order.PaymentProvider,
            order.PaymentTransactionId,
            order.PaymentMessage,
            order.AmountCaptured,
            order.AmountRefunded,
            order.Subtotal,
            order.CreatedAt,
            order.UpdatedAt,
            order.Items
                .OrderBy(i => i.ProductName)
                .Select(i => new OrderLineItemDto(
                    i.Id,
                    i.ProductId,
                    i.ProductVariantId,
                    i.Sku,
                    i.ProductName,
                    i.Quantity,
                    i.UnitPrice,
                    i.LineTotal))
                .ToList());

    private static OrderAddressDto MapAddress(OrderAddress address) =>
        new(
            address.Name,
            address.Line1,
            address.Line2,
            address.City,
            address.Region,
            address.PostalCode,
            address.Country);

    private static OrderHistoryEntryDto MapHistory(OrderHistoryEntry entry) =>
        new(
            entry.Id,
            entry.OrderId,
            entry.EventType,
            entry.PreviousStatus,
            entry.NewStatus,
            entry.Message,
            entry.Actor,
            entry.CreatedAt);
}