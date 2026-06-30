using Lumen.Application.Customers.Dtos;

namespace Lumen.Application.Customers;

public interface ICustomerService
{
    Task<CustomerDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CustomerDto?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CustomerDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<CustomerDto> EnsureCustomerForUserAsync(string userId, string email, string? displayName = null, CancellationToken cancellationToken = default);
}