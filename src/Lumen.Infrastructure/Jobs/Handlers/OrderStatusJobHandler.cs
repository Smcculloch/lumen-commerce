using Lumen.Application.Jobs;
using Lumen.Application.Orders;
using Lumen.Domain.Enums;
using Lumen.Domain.Jobs;
using Lumen.Domain.Orders;
using Microsoft.Extensions.Options;

namespace Lumen.Infrastructure.Jobs.Handlers;

public sealed class OrderStatusJobHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderHistoryRepository _historyRepository;
    private readonly INotificationLogRepository _notificationLogRepository;
    private readonly JobOptions _options;

    public OrderStatusJobHandler(
        IOrderRepository orderRepository,
        IOrderHistoryRepository historyRepository,
        INotificationLogRepository notificationLogRepository,
        IOptions<JobOptions> options)
    {
        _orderRepository = orderRepository;
        _historyRepository = historyRepository;
        _notificationLogRepository = notificationLogRepository;
        _options = options.Value;
    }

    public async Task<string> ExecuteAsync(CancellationToken cancellationToken)
    {
        var olderThan = DateTimeOffset.UtcNow.AddHours(-_options.OrderStatus.PendingHours);
        var orders = await _orderRepository.ListStalePendingAsync(olderThan, cancellationToken);

        var notified = 0;
        foreach (var order in orders)
        {
            var subject = $"Order {order.OrderNumber} status update";
            var body =
                $"Your order {order.OrderNumber} is still {order.Status}. " +
                "We will notify you when it progresses.";

            await _notificationLogRepository.AddAsync(
                NotificationLog.Create(
                    "OrderStatus",
                    order.Email,
                    subject,
                    body,
                    order.Id),
                cancellationToken);

            await _historyRepository.AddAsync(
                OrderHistoryEntry.Create(
                    order.Id,
                    OrderHistoryEventType.Notification,
                    "Automated status reminder queued (stub).",
                    actor: "system:order-status-job"),
                cancellationToken);

            notified++;
        }

        if (notified > 0)
        {
            await _notificationLogRepository.SaveChangesAsync(cancellationToken);
            await _historyRepository.SaveChangesAsync(cancellationToken);
        }

        return $"Processed {orders.Count} stale order(s); queued {notified} status notification(s).";
    }
}