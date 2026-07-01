using Lumen.Application.Jobs;
using Lumen.Application.Jobs.Dtos;
using Lumen.Domain.Jobs;
using Lumen.Infrastructure.Jobs.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Lumen.Infrastructure.Jobs;

public sealed class JobRunner : IJobRunner
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IJobExecutionRepository _executionRepository;

    public JobRunner(IServiceProvider serviceProvider, IJobExecutionRepository executionRepository)
    {
        _serviceProvider = serviceProvider;
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

        var schedulerFactory = _serviceProvider.GetService<ISchedulerFactory>();
        if (schedulerFactory is not null)
        {
            var scheduler = await schedulerFactory.GetScheduler(cancellationToken);
            var job = new JobKey(jobKey);

            if (await scheduler.CheckExists(job, cancellationToken))
            {
                await scheduler.TriggerJob(job, cancellationToken);
                return new JobTriggerResult(true, $"Job '{jobKey}' triggered.");
            }
        }

        return await RunHandlerDirectlyAsync(jobKey, cancellationToken);
    }

    private async Task<JobTriggerResult> RunHandlerDirectlyAsync(string jobKey, CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var logger = scope.ServiceProvider.GetRequiredService<JobExecutionLogger>();

        try
        {
            var message = await logger.RunAsync(
                jobKey,
                () => ResolveHandler(scope.ServiceProvider, jobKey, cancellationToken),
                cancellationToken);

            return new JobTriggerResult(true, message);
        }
        catch (Exception ex)
        {
            return new JobTriggerResult(false, ex.Message);
        }
    }

    private static Task<string> ResolveHandler(
        IServiceProvider provider,
        string jobKey,
        CancellationToken cancellationToken) =>
        jobKey switch
        {
            LumenJobKeys.AbandonedCart =>
                provider.GetRequiredService<AbandonedCartJobHandler>().ExecuteAsync(cancellationToken),
            LumenJobKeys.CartCleanup =>
                provider.GetRequiredService<CartCleanupJobHandler>().ExecuteAsync(cancellationToken),
            LumenJobKeys.OrderStatus =>
                provider.GetRequiredService<OrderStatusJobHandler>().ExecuteAsync(cancellationToken),
            LumenJobKeys.InventorySync =>
                provider.GetRequiredService<InventorySyncJobHandler>().ExecuteAsync(cancellationToken),
            _ => throw new InvalidOperationException($"No handler registered for job '{jobKey}'.")
        };

    private static JobExecutionDto Map(JobExecution execution) =>
        new(
            execution.Id,
            execution.JobKey,
            execution.Status,
            execution.Message,
            execution.StartedAt,
            execution.CompletedAt);
}