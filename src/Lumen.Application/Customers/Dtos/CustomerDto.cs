namespace Lumen.Application.Customers.Dtos;

public sealed record CustomerDto(
    Guid Id,
    string UserId,
    string Email,
    string? DisplayName,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);