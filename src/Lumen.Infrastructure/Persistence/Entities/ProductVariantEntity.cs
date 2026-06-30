using Lumen.Domain.Enums;

namespace Lumen.Infrastructure.Persistence.Entities;

/// <summary>
/// EF persistence model for <see cref="Domain.Products.ProductVariant"/>.
/// </summary>
public sealed class ProductVariantEntity
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public ProductStatus Status { get; set; } = ProductStatus.Draft;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string PropertiesJson { get; set; } = "{}";

    public ProductEntity Product { get; set; } = null!;
}