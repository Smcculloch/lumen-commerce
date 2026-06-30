using Lumen.Domain.Enums;

namespace Lumen.Infrastructure.Persistence.Entities;

public sealed class OrderEntity
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid? CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ShippingName { get; set; } = string.Empty;
    public string ShippingLine1 { get; set; } = string.Empty;
    public string? ShippingLine2 { get; set; }
    public string ShippingCity { get; set; } = string.Empty;
    public string? ShippingRegion { get; set; }
    public string ShippingPostalCode { get; set; } = string.Empty;
    public string ShippingCountry { get; set; } = string.Empty;
    public string BillingName { get; set; } = string.Empty;
    public string BillingLine1 { get; set; } = string.Empty;
    public string? BillingLine2 { get; set; }
    public string BillingCity { get; set; } = string.Empty;
    public string? BillingRegion { get; set; }
    public string BillingPostalCode { get; set; } = string.Empty;
    public string BillingCountry { get; set; } = string.Empty;
    public string? OrderNotes { get; set; }
    public OrderStatus Status { get; set; }
    public decimal Subtotal { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public CustomerEntity? Customer { get; set; }
    public ICollection<OrderLineItemEntity> Items { get; set; } = new List<OrderLineItemEntity>();
}