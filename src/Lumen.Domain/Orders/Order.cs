using Lumen.Domain.Enums;

namespace Lumen.Domain.Orders;

/// <summary>
/// A customer or guest order created from the shopping cart at checkout.
/// </summary>
public sealed class Order
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string OrderNumber { get; private set; } = string.Empty;
    public Guid? CustomerId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public OrderAddress ShippingAddress { get; private set; } = null!;
    public OrderAddress BillingAddress { get; private set; } = null!;
    public string? OrderNotes { get; private set; }
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;
    public PaymentStatus PaymentStatus { get; private set; } = PaymentStatus.None;
    public string? PaymentProvider { get; private set; }
    public string? PaymentTransactionId { get; private set; }
    public string? PaymentMessage { get; private set; }
    public decimal AmountCaptured { get; private set; }
    public decimal AmountRefunded { get; private set; }
    public decimal Subtotal { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public List<OrderLineItem> Items { get; private set; } = [];

    public static Order Create(
        string orderNumber,
        Guid? customerId,
        string customerName,
        string email,
        OrderAddress shippingAddress,
        OrderAddress billingAddress,
        string? orderNotes,
        IReadOnlyList<OrderLineInput> lines)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(orderNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(customerName);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        if (lines.Count == 0)
        {
            throw new InvalidOperationException("An order must contain at least one line item.");
        }

        var order = new Order
        {
            OrderNumber = orderNumber.Trim(),
            CustomerId = customerId,
            CustomerName = customerName.Trim(),
            Email = email.Trim(),
            ShippingAddress = shippingAddress,
            BillingAddress = billingAddress,
            OrderNotes = string.IsNullOrWhiteSpace(orderNotes) ? null : orderNotes.Trim(),
            Subtotal = lines.Sum(i => i.UnitPrice * i.Quantity),
            PaymentStatus = PaymentStatus.Pending
        };

        foreach (var line in lines)
        {
            order.Items.Add(OrderLineItem.Create(
                order.Id,
                line.ProductId,
                line.ProductVariantId,
                line.Sku,
                line.ProductName,
                line.Quantity,
                line.UnitPrice));
        }

        return order;
    }

    public readonly record struct OrderLineInput(
        Guid ProductId,
        Guid? ProductVariantId,
        string Sku,
        string ProductName,
        int Quantity,
        decimal UnitPrice);

    public static Order Rehydrate(
        Guid id,
        string orderNumber,
        Guid? customerId,
        string customerName,
        string email,
        OrderAddress shippingAddress,
        OrderAddress billingAddress,
        string? orderNotes,
        OrderStatus status,
        PaymentStatus paymentStatus,
        string? paymentProvider,
        string? paymentTransactionId,
        string? paymentMessage,
        decimal amountCaptured,
        decimal amountRefunded,
        decimal subtotal,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        IEnumerable<OrderLineItem> items) =>
        new()
        {
            Id = id,
            OrderNumber = orderNumber,
            CustomerId = customerId,
            CustomerName = customerName,
            Email = email,
            ShippingAddress = shippingAddress,
            BillingAddress = billingAddress,
            OrderNotes = orderNotes,
            Status = status,
            PaymentStatus = paymentStatus,
            PaymentProvider = paymentProvider,
            PaymentTransactionId = paymentTransactionId,
            PaymentMessage = paymentMessage,
            AmountCaptured = amountCaptured,
            AmountRefunded = amountRefunded,
            Subtotal = subtotal,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            Items = items.ToList()
        };

    public void SetStatus(OrderStatus status)
    {
        Status = status;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Cancel()
    {
        if (Status is OrderStatus.Shipped or OrderStatus.Completed)
        {
            throw new InvalidOperationException("Shipped or completed orders cannot be cancelled.");
        }

        Status = OrderStatus.Cancelled;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ApplyPaymentOutcome(
        string providerName,
        PaymentStatus paymentStatus,
        string? transactionId,
        string? message,
        bool paymentSucceeded)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerName);

        PaymentProvider = providerName.Trim();
        PaymentStatus = paymentStatus;
        PaymentTransactionId = transactionId;
        PaymentMessage = message;

        if (paymentSucceeded)
        {
            Status = OrderStatus.Processing;
            AmountCaptured = Subtotal;
        }

        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ApplyPaymentFailure(string providerName, string? message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerName);

        PaymentProvider = providerName.Trim();
        PaymentStatus = PaymentStatus.Failed;
        PaymentMessage = message;
        Status = OrderStatus.Pending;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ApplyCapture(decimal amount, PaymentStatus paymentStatus, string? message)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Capture amount cannot be negative.");
        }

        AmountCaptured = amount;
        PaymentStatus = paymentStatus;
        PaymentMessage = message;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ApplyRefund(decimal amount, PaymentStatus paymentStatus, string? message)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Refund amount cannot be negative.");
        }

        AmountRefunded += amount;
        PaymentStatus = paymentStatus;
        PaymentMessage = message;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}