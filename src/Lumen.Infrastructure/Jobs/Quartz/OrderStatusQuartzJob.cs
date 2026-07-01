using Lumen.Infrastructure.Jobs.Handlers;
using Quartz;

namespace Lumen.Infrastructure.Jobs.Quartz;

[DisallowConcurrentExecution]
public sealed class OrderStatusQuartzJob : IJob
{
    private readonly OrderStatusJobHandler _handler;
    private readonly JobExecutionLogger _logger;

    public OrderStatusQuartzJob(OrderStatusJobHandler handler, JobExecutionLogger logger)
    {
        _handler = handler;
        _logger = logger;
    }

    public Task Execute(IJobExecutionContext context) =>
        _logger.RunAsync(
            LumenJobKeys.OrderStatus,
            () => _handler.ExecuteAsync(context.CancellationToken),
            context.CancellationToken);
}