namespace Lumen.Application.Products.Dtos;

public sealed record UpdateVariantRequest(
    string Sku,
    string Name,
    int SortOrder,
    IDictionary<string, object?> Properties);