using Lumen.Application.Jobs;
using Lumen.Domain.Jobs;
using Lumen.Infrastructure.Persistence;
using Lumen.Infrastructure.Persistence.Mapping;
using Microsoft.EntityFrameworkCore;

namespace Lumen.Infrastructure.Repositories;

public sealed class NotificationLogRepository : INotificationLogRepository
{
    private readonly AppDbContext _dbContext;

    public NotificationLogRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(NotificationLog notification, CancellationToken cancellationToken = default) =>
        await _dbContext.NotificationLogs.AddAsync(JobMapping.ToEntity(notification), cancellationToken);

    public async Task<IReadOnlyList<NotificationLog>> ListRecentAsync(int take, CancellationToken cancellationToken = default)
    {
        var entities = await _dbContext.NotificationLogs.ToListAsync(cancellationToken);

        return entities
            .OrderByDescending(x => x.CreatedAt)
            .Take(take)
            .Select(JobMapping.ToDomain)
            .ToList();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}