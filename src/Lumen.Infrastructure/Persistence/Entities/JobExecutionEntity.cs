using Lumen.Domain.Jobs;

namespace Lumen.Infrastructure.Persistence.Entities;

public sealed class JobExecutionEntity
{
    public Guid Id { get; set; }
    public string JobKey { get; set; } = string.Empty;
    public JobExecutionStatus Status { get; set; }
    public string? Message { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}