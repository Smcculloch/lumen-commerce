namespace Lumen.Domain.Cart;

/// <summary>
/// A line item in a shopping cart referencing PIM product data.
/// </summary>
public sealed class CartItem
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid CartId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid? ProductVariantId { get; private set; }
    public string Sku { get; private set; } = string.Empty;
    public string ProductName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public decimal LineTotal => UnitPrice * Quantity;

    public static CartItem Create(
        Guid cartId,
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

        return new CartItem
        {
            CartId = cartId,
            ProductId = productId,
            ProductVariantId = productVariantId,
            Sku = sku.Trim(),
            ProductName = productName.Trim(),
            Quantity = quantity,
            UnitPrice = unitPrice
        };
    }

    public static CartItem Rehydrate(
        Guid id,
        Guid cartId,
        Guid productId,
        Guid? productVariantId,
        string sku,
        string productName,
        int quantity,
        decimal unitPrice,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt) =>
        new()
        {
            Id = id,
            CartId = cartId,
            ProductId = productId,
            ProductVariantId = productVariantId,
            Sku = sku,
            ProductName = productName,
            Quantity = quantity,
            UnitPrice = unitPrice,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

    public void SetQuantity(int quantity)
    {
        if (quantity < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be at least 1.");
        }

        Quantity = quantity;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}