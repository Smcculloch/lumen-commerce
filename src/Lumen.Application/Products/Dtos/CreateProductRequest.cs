namespace Lumen.Application.Products.Dtos;

public sealed record CreateProductRequest(
    string TemplateKey,
    string Sku,
    string Name,
    Guid? CategoryId,
    int SortOrder,
    IDictionary<string, object?> Properties);