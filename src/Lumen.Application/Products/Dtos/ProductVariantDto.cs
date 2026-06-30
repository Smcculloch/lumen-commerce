using Lumen.Domain.Enums;

namespace Lumen.Application.Products.Dtos;

public sealed record ProductVariantDto(
    Guid Id,
    Guid ProductId,
    string Sku,
    string Name,
    int SortOrder,
    ProductStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyDictionary<string, object?> Properties);