using Lumen.Application.Customers.Dtos;
using Lumen.Domain.Customers;

namespace Lumen.Application.Customers;

public sealed class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _repository;

    public CustomerService(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<CustomerDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _repository.GetByIdAsync(id, cancellationToken);
        return customer is null ? null : Map(customer);
    }

    public async Task<CustomerDto?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var customer = await _repository.GetByUserIdAsync(userId, cancellationToken);
        return customer is null ? null : Map(customer);
    }

    public async Task<IReadOnlyList<CustomerDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var customers = await _repository.ListAsync(cancellationToken);
        return customers.Select(Map).ToList();
    }

    public async Task<CustomerDto> EnsureCustomerForUserAsync(
        string userId,
        string email,
        string? displayName = null,
        CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByUserIdAsync(userId, cancellationToken);
        if (existing is not null)
        {
            existing.UpdateProfile(email, displayName);
            await _repository.UpdateAsync(existing, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            return Map(existing);
        }

        var customer = Customer.Create(userId, email, displayName);
        await _repository.AddAsync(customer, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return Map(customer);
    }

    private static CustomerDto Map(Customer customer) =>
        new(
            customer.Id,
            customer.UserId,
            customer.Email,
            customer.DisplayName,
            customer.CreatedAt,
            customer.UpdatedAt);
}