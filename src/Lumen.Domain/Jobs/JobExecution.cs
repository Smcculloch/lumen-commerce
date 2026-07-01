namespace Lumen.Domain.Jobs;

public enum JobExecutionStatus
{
    Running = 0,
    Succeeded = 1,
    Failed = 2
}

public sealed class JobExecution
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string JobKey { get; private set; } = string.Empty;
    public JobExecutionStatus Status { get; private set; } = JobExecutionStatus.Running;
    public string? Message { get; private set; }
    public DateTimeOffset StartedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CompletedAt { get; private set; }

    public static JobExecution Start(string jobKey) =>
        new()
        {
            JobKey = jobKey.Trim()
        };

    public static JobExecution Rehydrate(
        Guid id,
        string jobKey,
        JobExecutionStatus status,
        string? message,
        DateTimeOffset startedAt,
        DateTimeOffset? completedAt) =>
        new()
        {
            Id = id,
            JobKey = jobKey,
            Status = status,
            Message = message,
            StartedAt = startedAt,
            CompletedAt = completedAt
        };

    public void Complete(string? message)
    {
        Status = JobExecutionStatus.Succeeded;
        Message = message;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    public void Fail(string? message)
    {
        Status = JobExecutionStatus.Failed;
        Message = message;
        CompletedAt = DateTimeOffset.UtcNow;
    }
}