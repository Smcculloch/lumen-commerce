using Lumen.Domain.Jobs;

namespace Lumen.Application.Jobs.Dtos;

public sealed record JobDefinitionDto(
    string Key,
    string DisplayName,
    string Description,
    string CronExpression);

public sealed record JobExecutionDto(
    Guid Id,
    string JobKey,
    JobExecutionStatus Status,
    string? Message,
    DateTimeOffset StartedAt,
    DateTimeOffset? CompletedAt);

public sealed record JobTriggerResult(
    bool Succeeded,
    string Message);