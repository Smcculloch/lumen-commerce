namespace Lumen.Domain.Jobs;

public sealed class NotificationLog
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Type { get; private set; } = string.Empty;
    public string Recipient { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public Guid? RelatedEntityId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public static NotificationLog Create(
        string type,
        string recipient,
        string subject,
        string body,
        Guid? relatedEntityId = null) =>
        new()
        {
            Type = type.Trim(),
            Recipient = recipient.Trim(),
            Subject = subject.Trim(),
            Body = body.Trim(),
            RelatedEntityId = relatedEntityId
        };

    public static NotificationLog Rehydrate(
        Guid id,
        string type,
        string recipient,
        string subject,
        string body,
        Guid? relatedEntityId,
        DateTimeOffset createdAt) =>
        new()
        {
            Id = id,
            Type = type,
            Recipient = recipient,
            Subject = subject,
            Body = body,
            RelatedEntityId = relatedEntityId,
            CreatedAt = createdAt
        };
}