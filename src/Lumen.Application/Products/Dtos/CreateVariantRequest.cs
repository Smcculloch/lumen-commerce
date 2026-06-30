namespace Lumen.Application.Products.Dtos;

public sealed record CreateVariantRequest(
    string Sku,
    string Name,
    int SortOrder,
    IDictionary<string, object?> Properties);