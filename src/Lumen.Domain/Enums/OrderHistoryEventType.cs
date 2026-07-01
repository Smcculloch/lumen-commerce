namespace Lumen.Domain.Enums;

public enum OrderHistoryEventType
{
    Created = 0,
    StatusChanged = 1,
    PaymentEvent = 2,
    Note = 3,
    Notification = 4,
    Cancelled = 5
}