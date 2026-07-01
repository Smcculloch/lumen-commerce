using Lumen.Application.Jobs;
using Lumen.Application.Jobs.Dtos;
using Lumen.Domain.Jobs;
using Quartz;

namespace Lumen.Infrastructure.Jobs;

public sealed class JobRunner : IJobRunner
{
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly IJobExecutionRepository _executionRepository;

    public JobRunner(ISchedulerFactory schedulerFactory, IJobExecutionRepository executionRepository)
    {
        _schedulerFactory = schedulerFactory;
        _executionRepository = executionRepository;
    }

    public IReadOnlyList<JobDefinitionDto> ListJobs() => JobRegistry.Definitions;

    public async Task<IReadOnlyList<JobExecutionDto>> ListRecentExecutionsAsync(
        int take = 20,
        CancellationToken cancellationToken = default)
    {
        var executions = await _executionRepository.ListRecentAsync(take, cancellationToken);
        return executions.Select(Map).ToList();
    }

    public async Task<JobExecutionDto?> GetLatestExecutionAsync(
        string jobKey,
        CancellationToken cancellationToken = default)
    {
        var execution = await _executionRepository.GetLatestForJobAsync(jobKey, cancellationToken);
        return execution is null ? null : Map(execution);
    }

    public async Task<JobTriggerResult> TriggerAsync(string jobKey, CancellationToken cancellationToken = default)
    {
        if (!JobRegistry.Definitions.Any(d => string.Equals(d.Key, jobKey, StringComparison.OrdinalIgnoreCase)))
        {
            return new JobTriggerResult(false, $"Unknown job '{jobKey}'.");
        }

        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
        var job = new JobKey(jobKey);

        if (!await scheduler.CheckExists(job, cancellationToken))
        {
            return new JobTriggerResult(false, $"Job '{jobKey}' is not registered with the scheduler.");
        }

        await scheduler.TriggerJob(job, cancellationToken);
        return new JobTriggerResult(true, $"Job '{jobKey}' triggered.");
    }

    private static JobExecutionDto Map(JobExecution execution) =>
        new(
            execution.Id,
            execution.JobKey,
            execution.Status,
            execution.Message,
            execution.StartedAt,
            execution.CompletedAt);
}