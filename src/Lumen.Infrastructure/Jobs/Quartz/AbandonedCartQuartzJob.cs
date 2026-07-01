using Lumen.Infrastructure.Jobs.Handlers;
using Quartz;

namespace Lumen.Infrastructure.Jobs.Quartz;

[DisallowConcurrentExecution]
public sealed class AbandonedCartQuartzJob : IJob
{
    private readonly AbandonedCartJobHandler _handler;
    private readonly JobExecutionLogger _logger;

    public AbandonedCartQuartzJob(AbandonedCartJobHandler handler, JobExecutionLogger logger)
    {
        _handler = handler;
        _logger = logger;
    }

    public Task Execute(IJobExecutionContext context) =>
        _logger.RunAsync(
            LumenJobKeys.AbandonedCart,
            () => _handler.ExecuteAsync(context.CancellationToken),
            context.CancellationToken);
}