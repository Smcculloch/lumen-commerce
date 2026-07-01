using Lumen.Infrastructure.Jobs.Handlers;
using Quartz;

namespace Lumen.Infrastructure.Jobs.Quartz;

[DisallowConcurrentExecution]
public sealed class InventorySyncQuartzJob : IJob
{
    private readonly InventorySyncJobHandler _handler;
    private readonly JobExecutionLogger _logger;

    public InventorySyncQuartzJob(InventorySyncJobHandler handler, JobExecutionLogger logger)
    {
        _handler = handler;
        _logger = logger;
    }

    public Task Execute(IJobExecutionContext context) =>
        _logger.RunAsync(
            LumenJobKeys.InventorySync,
            () => _handler.ExecuteAsync(context.CancellationToken),
            context.CancellationToken);
}