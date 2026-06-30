using Lumen.Application.Customers;
using Lumen.Domain.Customers;
using Lumen.Infrastructure.Persistence;
using Lumen.Infrastructure.Persistence.Mapping;
using Microsoft.EntityFrameworkCore;

namespace Lumen.Infrastructure.Repositories;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _dbContext;

    public CustomerRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Customers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return entity is null ? null : InstanceMapping.ToDomain(entity);
    }

    public async Task<Customer?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Customers.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        return entity is null ? null : InstanceMapping.ToDomain(entity);
    }

    public async Task<IReadOnlyList<Customer>> ListAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _dbContext.Customers.ToListAsync(cancellationToken);

        return entities
            .OrderByDescending(x => x.CreatedAt)
            .Select(InstanceMapping.ToDomain)
            .ToList();
    }

    public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        await _dbContext.Customers.AddAsync(InstanceMapping.ToEntity(customer), cancellationToken);
    }

    public async Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Customers.FindAsync([customer.Id], cancellationToken)
            ?? throw new InvalidOperationException($"Customer '{customer.Id}' was not found.");

        _dbContext.Entry(entity).CurrentValues.SetValues(InstanceMapping.ToEntity(customer));
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}