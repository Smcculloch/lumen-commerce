using Lumen.Domain.Jobs;

namespace Lumen.Application.Jobs;

public interface INotificationLogRepository
{
    Task AddAsync(NotificationLog notification, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NotificationLog>> ListRecentAsync(int take, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}