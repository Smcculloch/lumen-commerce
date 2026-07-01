namespace Lumen.Infrastructure.Persistence.Entities;

public sealed class NotificationLogEntity
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public Guid? RelatedEntityId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}