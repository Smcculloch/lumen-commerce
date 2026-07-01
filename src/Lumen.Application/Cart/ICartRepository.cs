using Lumen.Domain.Cart;

namespace Lumen.Application.Cart;

public interface ICartRepository
{
    Task<ShoppingCart?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ShoppingCart?> GetBySessionKeyAsync(string sessionKey, CancellationToken cancellationToken = default);
    Task<ShoppingCart?> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task AddAsync(ShoppingCart cart, CancellationToken cancellationToken = default);
    Task UpdateAsync(ShoppingCart cart, CancellationToken cancellationToken = default);
    Task DeleteAsync(ShoppingCart cart, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ShoppingCart>> ListInactiveWithItemsAsync(DateTimeOffset inactiveSince, CancellationToken cancellationToken = default);
    Task<int> DeleteStaleCartsAsync(DateTimeOffset olderThan, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}