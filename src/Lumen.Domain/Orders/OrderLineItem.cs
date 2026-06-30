namespace Lumen.Domain.Orders;

/// <summary>
/// A line item on a placed order with snapshotted product data.
/// </summary>
public sealed class OrderLineItem
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid? ProductVariantId { get; private set; }
    public string Sku { get; private set; } = string.Empty;
    public string ProductName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    public decimal LineTotal => UnitPrice * Quantity;

    public static OrderLineItem Create(
        Guid orderId,
        Guid productId,
        Guid? productVariantId,
        string sku,
        string productName,
        int quantity,
        decimal unitPrice)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        ArgumentException.ThrowIfNullOrWhiteSpace(productName);

        if (quantity < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be at least 1.");
        }

        if (unitPrice < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(unitPrice), "Unit price cannot be negative.");
        }

        return new OrderLineItem
        {
            OrderId = orderId,
            ProductId = productId,
            ProductVariantId = productVariantId,
            Sku = sku.Trim(),
            ProductName = productName.Trim(),
            Quantity = quantity,
            UnitPrice = unitPrice
        };
    }

    public static OrderLineItem Rehydrate(
        Guid id,
        Guid orderId,
        Guid productId,
        Guid? productVariantId,
        string sku,
        string productName,
        int quantity,
        decimal unitPrice) =>
        new()
        {
            Id = id,
            OrderId = orderId,
            ProductId = productId,
            ProductVariantId = productVariantId,
            Sku = sku,
            ProductName = productName,
            Quantity = quantity,
            UnitPrice = unitPrice
        };
}