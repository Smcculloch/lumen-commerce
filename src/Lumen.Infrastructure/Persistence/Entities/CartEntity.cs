namespace Lumen.Infrastructure.Persistence.Entities;

public sealed class CartEntity
{
    public Guid Id { get; set; }
    public Guid? CustomerId { get; set; }
    public string? SessionKey { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public CustomerEntity? Customer { get; set; }
    public ICollection<CartItemEntity> Items { get; set; } = new List<CartItemEntity>();
}