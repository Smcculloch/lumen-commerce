using Lumen.Domain.Enums;

namespace Lumen.Domain.Products;

/// <summary>
/// A PIM product instance created from a resolved template definition.
/// </summary>
public sealed class Product
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string TemplateKey { get; private set; } = string.Empty;
    public string Sku { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public Guid? CategoryId { get; private set; }
    public int SortOrder { get; private set; }
    public ProductStatus Status { get; private set; } = ProductStatus.Draft;
    public DateTimeOffset? PublishedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public Dictionary<string, object?> Properties { get; private set; } = new(StringComparer.OrdinalIgnoreCase);

    public bool IsPublished => Status == ProductStatus.Published;

    public static Product Create(
        string templateKey,
        string sku,
        string name,
        Guid? categoryId,
        int sortOrder,
        IDictionary<string, object?>? properties = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templateKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var product = new Product
        {
            TemplateKey = templateKey.Trim().ToLowerInvariant(),
            Sku = sku.Trim(),
            Name = name.Trim(),
            CategoryId = categoryId,
            SortOrder = sortOrder
        };

        if (properties is not null)
        {
            foreach (var (key, value) in properties)
            {
                product.Properties[key] = value;
            }
        }

        return product;
    }

    public static Product Rehydrate(
        Guid id,
        string templateKey,
        string sku,
        string name,
        Guid? categoryId,
        int sortOrder,
        ProductStatus status,
        DateTimeOffset? publishedAt,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        IDictionary<string, object?> properties) =>
        new()
        {
            Id = id,
            TemplateKey = templateKey,
            Sku = sku,
            Name = name,
            CategoryId = categoryId,
            SortOrder = sortOrder,
            Status = status,
            PublishedAt = publishedAt,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            Properties = new Dictionary<string, object?>(properties, StringComparer.OrdinalIgnoreCase)
        };

    public void Update(string sku, string name, Guid? categoryId, int sortOrder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Sku = sku.Trim();
        Name = name.Trim();
        CategoryId = categoryId;
        SortOrder = sortOrder;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetProperties(IDictionary<string, object?> properties)
    {
        Properties = new Dictionary<string, object?>(properties, StringComparer.OrdinalIgnoreCase);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Publish()
    {
        Status = ProductStatus.Published;
        PublishedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Unpublish()
    {
        Status = ProductStatus.Draft;
        PublishedAt = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Archive()
    {
        Status = ProductStatus.Archived;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}