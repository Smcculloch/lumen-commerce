using Lumen.Domain.Enums;

namespace Lumen.Domain.Orders;

public sealed class OrderHistoryEntry
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid OrderId { get; private set; }
    public OrderHistoryEventType EventType { get; private set; }
    public OrderStatus? PreviousStatus { get; private set; }
    public OrderStatus? NewStatus { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public string? Actor { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public static OrderHistoryEntry Create(
        Guid orderId,
        OrderHistoryEventType eventType,
        string message,
        OrderStatus? previousStatus = null,
        OrderStatus? newStatus = null,
        string? actor = null) =>
        new()
        {
            OrderId = orderId,
            EventType = eventType,
            Message = message.Trim(),
            PreviousStatus = previousStatus,
            NewStatus = newStatus,
            Actor = string.IsNullOrWhiteSpace(actor) ? null : actor.Trim()
        };

    public static OrderHistoryEntry Rehydrate(
        Guid id,
        Guid orderId,
        OrderHistoryEventType eventType,
        OrderStatus? previousStatus,
        OrderStatus? newStatus,
        string message,
        string? actor,
        DateTimeOffset createdAt) =>
        new()
        {
            Id = id,
            OrderId = orderId,
            EventType = eventType,
            PreviousStatus = previousStatus,
            NewStatus = newStatus,
            Message = message,
            Actor = actor,
            CreatedAt = createdAt
        };
}