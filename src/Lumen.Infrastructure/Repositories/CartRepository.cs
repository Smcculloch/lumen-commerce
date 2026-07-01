using Lumen.Application.Cart;
using Lumen.Domain.Cart;
using Lumen.Infrastructure.Persistence;
using Lumen.Infrastructure.Persistence.Mapping;
using Microsoft.EntityFrameworkCore;

namespace Lumen.Infrastructure.Repositories;

public sealed class CartRepository : ICartRepository
{
    private readonly AppDbContext _dbContext;

    public CartRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ShoppingCart?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Carts
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return entity is null ? null : InstanceMapping.ToDomain(entity);
    }

    public async Task<ShoppingCart?> GetBySessionKeyAsync(string sessionKey, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Carts
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.SessionKey == sessionKey, cancellationToken);

        return entity is null ? null : InstanceMapping.ToDomain(entity);
    }

    public async Task<ShoppingCart?> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Carts
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.CustomerId == customerId, cancellationToken);

        return entity is null ? null : InstanceMapping.ToDomain(entity);
    }

    public async Task AddAsync(ShoppingCart cart, CancellationToken cancellationToken = default)
    {
        await _dbContext.Carts.AddAsync(InstanceMapping.ToEntity(cart), cancellationToken);
    }

    public async Task UpdateAsync(ShoppingCart cart, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Carts
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == cart.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Cart '{cart.Id}' was not found.");

        entity.CustomerId = cart.CustomerId;
        entity.SessionKey = cart.SessionKey;
        entity.UpdatedAt = cart.UpdatedAt;

        var incomingIds = cart.Items.Select(i => i.Id).ToHashSet();
        var toRemove = entity.Items.Where(i => !incomingIds.Contains(i.Id)).ToList();
        foreach (var remove in toRemove)
        {
            entity.Items.Remove(remove);
        }

        foreach (var item in cart.Items)
        {
            var existing = entity.Items.FirstOrDefault(i => i.Id == item.Id);
            if (existing is null)
            {
                await _dbContext.CartItems.AddAsync(InstanceMapping.ToEntity(item), cancellationToken);
            }
            else
            {
                existing.ProductId = item.ProductId;
                existing.ProductVariantId = item.ProductVariantId;
                existing.Sku = item.Sku;
                existing.ProductName = item.ProductName;
                existing.Quantity = item.Quantity;
                existing.UnitPrice = item.UnitPrice;
                existing.UpdatedAt = item.UpdatedAt;
            }
        }
    }

    public async Task<IReadOnlyList<ShoppingCart>> ListInactiveWithItemsAsync(
        DateTimeOffset inactiveSince,
        CancellationToken cancellationToken = default)
    {
        var entities = await _dbContext.Carts
            .Include(x => x.Items)
            .Where(x => x.UpdatedAt <= inactiveSince)
            .ToListAsync(cancellationToken);

        return entities
            .Where(x => x.Items.Count > 0)
            .Select(InstanceMapping.ToDomain)
            .ToList();
    }

    public async Task<int> DeleteStaleCartsAsync(
        DateTimeOffset olderThan,
        CancellationToken cancellationToken = default)
    {
        var candidates = await _dbContext.Carts
            .Include(x => x.Items)
            .Where(x => x.UpdatedAt <= olderThan)
            .ToListAsync(cancellationToken);

        var stale = candidates.Where(x => x.Items.Count == 0).ToList();

        if (stale.Count == 0)
        {
            return 0;
        }

        _dbContext.Carts.RemoveRange(stale);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return stale.Count;
    }

    public async Task DeleteAsync(ShoppingCart cart, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Carts.FindAsync([cart.Id], cancellationToken);
        if (entity is not null)
        {
            _dbContext.Carts.Remove(entity);
        }
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}