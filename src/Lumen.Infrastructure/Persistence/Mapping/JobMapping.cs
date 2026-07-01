using Lumen.Domain.Jobs;
using Lumen.Domain.Orders;
using Lumen.Infrastructure.Persistence.Entities;

namespace Lumen.Infrastructure.Persistence.Mapping;

internal static class JobMapping
{
    public static OrderHistoryEntry ToDomain(OrderHistoryEntryEntity entity) =>
        OrderHistoryEntry.Rehydrate(
            entity.Id,
            entity.OrderId,
            entity.EventType,
            entity.PreviousStatus,
            entity.NewStatus,
            entity.Message,
            entity.Actor,
            entity.CreatedAt);

    public static OrderHistoryEntryEntity ToEntity(OrderHistoryEntry entry) =>
        new()
        {
            Id = entry.Id,
            OrderId = entry.OrderId,
            EventType = entry.EventType,
            PreviousStatus = entry.PreviousStatus,
            NewStatus = entry.NewStatus,
            Message = entry.Message,
            Actor = entry.Actor,
            CreatedAt = entry.CreatedAt
        };

    public static JobExecution ToDomain(JobExecutionEntity entity) =>
        JobExecution.Rehydrate(
            entity.Id,
            entity.JobKey,
            entity.Status,
            entity.Message,
            entity.StartedAt,
            entity.CompletedAt);

    public static JobExecutionEntity ToEntity(JobExecution execution) =>
        new()
        {
            Id = execution.Id,
            JobKey = execution.JobKey,
            Status = execution.Status,
            Message = execution.Message,
            StartedAt = execution.StartedAt,
            CompletedAt = execution.CompletedAt
        };

    public static NotificationLog ToDomain(NotificationLogEntity entity) =>
        NotificationLog.Rehydrate(
            entity.Id,
            entity.Type,
            entity.Recipient,
            entity.Subject,
            entity.Body,
            entity.RelatedEntityId,
            entity.CreatedAt);

    public static NotificationLogEntity ToEntity(NotificationLog notification) =>
        new()
        {
            Id = notification.Id,
            Type = notification.Type,
            Recipient = notification.Recipient,
            Subject = notification.Subject,
            Body = notification.Body,
            RelatedEntityId = notification.RelatedEntityId,
            CreatedAt = notification.CreatedAt
        };
}