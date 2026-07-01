using Lumen.Domain.Orders;

namespace Lumen.Application.Orders;

public interface IOrderHistoryRepository
{
    Task<IReadOnlyList<OrderHistoryEntry>> ListByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task AddAsync(OrderHistoryEntry entry, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}