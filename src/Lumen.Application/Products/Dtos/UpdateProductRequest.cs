namespace Lumen.Application.Products.Dtos;

public sealed record UpdateProductRequest(
    string Sku,
    string Name,
    Guid? CategoryId,
    int SortOrder,
    IDictionary<string, object?> Properties);