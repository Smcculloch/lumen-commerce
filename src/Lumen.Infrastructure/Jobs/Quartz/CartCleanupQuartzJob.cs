using Lumen.Infrastructure.Jobs.Handlers;
using Quartz;

namespace Lumen.Infrastructure.Jobs.Quartz;

[DisallowConcurrentExecution]
public sealed class CartCleanupQuartzJob : IJob
{
    private readonly CartCleanupJobHandler _handler;
    private readonly JobExecutionLogger _logger;

    public CartCleanupQuartzJob(CartCleanupJobHandler handler, JobExecutionLogger logger)
    {
        _handler = handler;
        _logger = logger;
    }

    public Task Execute(IJobExecutionContext context) =>
        _logger.RunAsync(
            LumenJobKeys.CartCleanup,
            () => _handler.ExecuteAsync(context.CancellationToken),
            context.CancellationToken);
}