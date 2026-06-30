using Lumen.Domain.Enums;

namespace Lumen.Domain.Content;

/// <summary>
/// A CMS content instance created from a resolved template definition.
/// Property values are stored in a flexible bag keyed by property name.
/// </summary>
public sealed class ContentItem
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string TemplateKey { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Slug { get; private set; }
    public Guid? ParentId { get; private set; }
    public int SortOrder { get; private set; }
    public int Level { get; private set; }

    /// <summary>
    /// GUID-based materialized path for efficient descendant queries (e.g. /{guid}/{guid}/).
    /// </summary>
    public string MaterializedPath { get; private set; } = "/";

    /// <summary>
    /// Slug-based URL path for storefront routing (e.g. /home/about).
    /// </summary>
    public string FullPath { get; private set; } = "/";

    public ContentStatus Status { get; private set; } = ContentStatus.Draft;
    public DateTimeOffset? PublishedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public Dictionary<string, object?> Properties { get; private set; } = new(StringComparer.OrdinalIgnoreCase);

    public bool IsPublished => Status == ContentStatus.Published;

    public static ContentItem Create(
        string templateKey,
        string name,
        string slug,
        Guid? parentId,
        int sortOrder,
        int level,
        string materializedPath,
        string fullPath,
        IDictionary<string, object?>? properties = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templateKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        ArgumentException.ThrowIfNullOrWhiteSpace(materializedPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(fullPath);

        var item = new ContentItem
        {
            TemplateKey = templateKey.Trim().ToLowerInvariant(),
            Name = name.Trim(),
            Slug = NormalizeSlug(slug),
            ParentId = parentId,
            SortOrder = sortOrder,
            Level = level,
            MaterializedPath = materializedPath,
            FullPath = fullPath
        };

        if (properties is not null)
        {
            foreach (var (key, value) in properties)
            {
                item.Properties[key] = value;
            }
        }

        return item;
    }

    /// <summary>
    /// Reconstructs a content item from persistence without re-validating property values.
    /// </summary>
    public static ContentItem Rehydrate(
        Guid id,
        string templateKey,
        string name,
        string? slug,
        Guid? parentId,
        int sortOrder,
        int level,
        string materializedPath,
        string fullPath,
        ContentStatus status,
        DateTimeOffset? publishedAt,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        IDictionary<string, object?> properties) =>
        new()
        {
            Id = id,
            TemplateKey = templateKey,
            Name = name,
            Slug = slug,
            ParentId = parentId,
            SortOrder = sortOrder,
            Level = level,
            MaterializedPath = materializedPath,
            FullPath = fullPath,
            Status = status,
            PublishedAt = publishedAt,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            Properties = new Dictionary<string, object?>(properties, StringComparer.OrdinalIgnoreCase)
        };

    public void UpdateMetadata(string name, string slug, int sortOrder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        Name = name.Trim();
        Slug = NormalizeSlug(slug);
        SortOrder = sortOrder;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetSortOrder(int sortOrder)
    {
        SortOrder = sortOrder;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetHierarchy(Guid? parentId, int level, string materializedPath, string fullPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(materializedPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(fullPath);

        ParentId = parentId;
        Level = level;
        MaterializedPath = materializedPath;
        FullPath = fullPath;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetProperties(IDictionary<string, object?> properties)
    {
        Properties = new Dictionary<string, object?>(properties, StringComparer.OrdinalIgnoreCase);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Publish()
    {
        Status = ContentStatus.Published;
        PublishedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Unpublish()
    {
        Status = ContentStatus.Draft;
        PublishedAt = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Archive()
    {
        Status = ContentStatus.Archived;
        PublishedAt = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static string NormalizeSlug(string slug) =>
        slug.Trim().ToLowerInvariant();
}