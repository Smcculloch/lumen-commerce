using Lumen.Application.Orders;
using Lumen.Domain.Orders;
using Lumen.Infrastructure.Persistence;
using Lumen.Infrastructure.Persistence.Mapping;
using Microsoft.EntityFrameworkCore;

namespace Lumen.Infrastructure.Repositories;

public sealed class OrderHistoryRepository : IOrderHistoryRepository
{
    private readonly AppDbContext _dbContext;

    public OrderHistoryRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<OrderHistoryEntry>> ListByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var entities = await _dbContext.OrderHistoryEntries
            .Where(x => x.OrderId == orderId)
            .ToListAsync(cancellationToken);

        return entities
            .OrderByDescending(x => x.CreatedAt)
            .Select(JobMapping.ToDomain)
            .ToList();
    }

    public async Task AddAsync(OrderHistoryEntry entry, CancellationToken cancellationToken = default) =>
        await _dbContext.OrderHistoryEntries.AddAsync(JobMapping.ToEntity(entry), cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}