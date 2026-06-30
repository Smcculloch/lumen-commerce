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
            Subtotal = lines.Sum(i => i.UnitPrice * i.Quantity)
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
}