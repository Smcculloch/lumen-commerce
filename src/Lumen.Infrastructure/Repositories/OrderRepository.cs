using Lumen.Application.Orders;
using Lumen.Application.Orders.Dtos;
using Lumen.Domain.Enums;
using Lumen.Domain.Orders;
using Lumen.Infrastructure.Persistence;
using Lumen.Infrastructure.Persistence.Entities;
using Lumen.Infrastructure.Persistence.Mapping;
using Microsoft.EntityFrameworkCore;

namespace Lumen.Infrastructure.Repositories;

public sealed class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _dbContext;

    public OrderRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Orders
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return entity is null ? null : InstanceMapping.ToDomain(entity);
    }

    public async Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Orders
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.OrderNumber == orderNumber, cancellationToken);

        return entity is null ? null : InstanceMapping.ToDomain(entity);
    }

    public async Task<IReadOnlyList<Order>> ListAsync(OrderListFilter filter, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Orders
            .Include(x => x.Items)
            .AsQueryable();

        if (filter.Status is { } status)
        {
            query = query.Where(x => x.Status == status);
        }

        if (filter.PaymentStatus is { } paymentStatus)
        {
            query = query.Where(x => x.PaymentStatus == paymentStatus);
        }

        if (filter.CustomerId is { } customerId)
        {
            query = query.Where(x => x.CustomerId == customerId);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();
            query = query.Where(x =>
                x.OrderNumber.Contains(search) ||
                x.Email.Contains(search) ||
                x.CustomerName.Contains(search));
        }

        var entities = await query.ToListAsync(cancellationToken);

        IEnumerable<OrderEntity> filtered = entities;

        if (filter.From is { } from)
        {
            filtered = filtered.Where(x => x.CreatedAt >= from);
        }

        if (filter.To is { } to)
        {
            filtered = filtered.Where(x => x.CreatedAt <= to);
        }

        return filtered
            .OrderByDescending(x => x.CreatedAt)
            .Select(InstanceMapping.ToDomain)
            .ToList();
    }

    public async Task<IReadOnlyList<Order>> ListStalePendingAsync(
        DateTimeOffset olderThan,
        CancellationToken cancellationToken = default)
    {
        var entities = await _dbContext.Orders
            .Include(x => x.Items)
            .Where(x => x.Status == OrderStatus.Pending || x.Status == OrderStatus.Processing)
            .ToListAsync(cancellationToken);

        return entities
            .Where(x => x.UpdatedAt <= olderThan)
            .Select(InstanceMapping.ToDomain)
            .ToList();
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default) =>
        _dbContext.Orders.CountAsync(cancellationToken);

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        var entity = InstanceMapping.ToEntity(order);
        entity.Items = order.Items.Select(InstanceMapping.ToEntity).ToList();
        await _dbContext.Orders.AddAsync(entity, cancellationToken);
    }

    public async Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Orders
            .FirstOrDefaultAsync(x => x.Id == order.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Order '{order.Id}' was not found.");

        entity.Status = order.Status;
        entity.PaymentStatus = order.PaymentStatus;
        entity.PaymentProvider = order.PaymentProvider;
        entity.PaymentTransactionId = order.PaymentTransactionId;
        entity.PaymentMessage = order.PaymentMessage;
        entity.AmountCaptured = order.AmountCaptured;
        entity.AmountRefunded = order.AmountRefunded;
        entity.UpdatedAt = order.UpdatedAt;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}