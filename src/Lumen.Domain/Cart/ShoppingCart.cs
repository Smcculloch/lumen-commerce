namespace Lumen.Domain.Cart;

/// <summary>
/// A shopping cart for anonymous (session) or authenticated customers.
/// </summary>
public sealed class ShoppingCart
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid? CustomerId { get; private set; }
    public string? SessionKey { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public List<CartItem> Items { get; private set; } = [];

    public int ItemCount => Items.Sum(i => i.Quantity);
    public decimal Subtotal => Items.Sum(i => i.LineTotal);

    public static ShoppingCart CreateForSession(string sessionKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionKey);
        return new ShoppingCart { SessionKey = sessionKey.Trim() };
    }

    public static ShoppingCart CreateForCustomer(Guid customerId) =>
        new() { CustomerId = customerId };

    public static ShoppingCart Rehydrate(
        Guid id,
        Guid? customerId,
        string? sessionKey,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        IEnumerable<CartItem> items) =>
        new()
        {
            Id = id,
            CustomerId = customerId,
            SessionKey = sessionKey,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            Items = items.ToList()
        };

    public void AttachToCustomer(Guid customerId)
    {
        CustomerId = customerId;
        SessionKey = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}