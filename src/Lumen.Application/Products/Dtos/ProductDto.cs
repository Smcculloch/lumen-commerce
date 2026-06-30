using Lumen.Domain.Enums;

namespace Lumen.Application.Products.Dtos;

public sealed record ProductDto(
    Guid Id,
    string TemplateKey,
    string Sku,
    string Name,
    Guid? CategoryId,
    int SortOrder,
    ProductStatus Status,
    DateTimeOffset? PublishedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyDictionary<string, object?> Properties,
    IReadOnlyList<ProductVariantDto> Variants);