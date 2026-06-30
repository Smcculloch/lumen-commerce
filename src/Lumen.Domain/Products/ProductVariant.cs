using Lumen.Domain.Enums;

namespace Lumen.Domain.Products;

/// <summary>
/// A sellable variant of a product with its own SKU and optional property overrides.
/// </summary>
public sealed class ProductVariant
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ProductId { get; private set; }
    public string Sku { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public int SortOrder { get; private set; }
    public ProductStatus Status { get; private set; } = ProductStatus.Draft;
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public Dictionary<string, object?> Properties { get; private set; } = new(StringComparer.OrdinalIgnoreCase);

    public static ProductVariant Create(
        Guid productId,
        string sku,
        string name,
        int sortOrder,
        IDictionary<string, object?>? properties = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var variant = new ProductVariant
        {
            ProductId = productId,
            Sku = sku.Trim(),
            Name = name.Trim(),
            SortOrder = sortOrder
        };

        if (properties is not null)
        {
            foreach (var (key, value) in properties)
            {
                variant.Properties[key] = value;
            }
        }

        return variant;
    }

    public static ProductVariant Rehydrate(
        Guid id,
        Guid productId,
        string sku,
        string name,
        int sortOrder,
        ProductStatus status,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        IDictionary<string, object?> properties) =>
        new()
        {
            Id = id,
            ProductId = productId,
            Sku = sku,
            Name = name,
            SortOrder = sortOrder,
            Status = status,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            Properties = new Dictionary<string, object?>(properties, StringComparer.OrdinalIgnoreCase)
        };

    public void Update(string sku, string name, int sortOrder, IDictionary<string, object?> properties)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Sku = sku.Trim();
        Name = name.Trim();
        SortOrder = sortOrder;
        Properties = new Dictionary<string, object?>(properties, StringComparer.OrdinalIgnoreCase);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Publish()
    {
        Status = ProductStatus.Published;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Unpublish()
    {
        Status = ProductStatus.Draft;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}