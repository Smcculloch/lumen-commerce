using Lumen.Domain.Enums;

namespace Lumen.Infrastructure.Persistence.Entities;

/// <summary>
/// EF persistence model for <see cref="Domain.Products.Product"/>.
/// </summary>
public sealed class ProductEntity
{
    public Guid Id { get; set; }
    public string TemplateKey { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid? CategoryId { get; set; }
    public int SortOrder { get; set; }
    public ProductStatus Status { get; set; } = ProductStatus.Draft;
    public DateTimeOffset? PublishedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string PropertiesJson { get; set; } = "{}";

    public CategoryEntity? Category { get; set; }
    public ICollection<ProductVariantEntity> Variants { get; set; } = new List<ProductVariantEntity>();
}