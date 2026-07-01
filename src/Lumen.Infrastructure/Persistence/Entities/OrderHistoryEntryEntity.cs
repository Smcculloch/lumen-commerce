using Lumen.Domain.Enums;

namespace Lumen.Infrastructure.Persistence.Entities;

public sealed class OrderHistoryEntryEntity
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public OrderHistoryEventType EventType { get; set; }
    public OrderStatus? PreviousStatus { get; set; }
    public OrderStatus? NewStatus { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Actor { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public OrderEntity? Order { get; set; }
}