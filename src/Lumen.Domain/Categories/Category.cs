namespace Lumen.Domain.Categories;

/// <summary>
/// Hierarchical product category for organizing the PIM catalog.
/// </summary>
public sealed class Category
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public Guid? ParentId { get; private set; }
    public int SortOrder { get; private set; }
    public int Level { get; private set; }
    public string MaterializedPath { get; private set; } = "/";
    public string FullPath { get; private set; } = "/";
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public static Category Create(
        string name,
        string slug,
        Guid? parentId,
        int sortOrder,
        int level,
        string materializedPath,
        string fullPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        ArgumentException.ThrowIfNullOrWhiteSpace(materializedPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(fullPath);

        return new Category
        {
            Name = name.Trim(),
            Slug = slug.Trim().ToLowerInvariant(),
            ParentId = parentId,
            SortOrder = sortOrder,
            Level = level,
            MaterializedPath = materializedPath,
            FullPath = fullPath
        };
    }

    public static Category Rehydrate(
        Guid id,
        string name,
        string slug,
        Guid? parentId,
        int sortOrder,
        int level,
        string materializedPath,
        string fullPath,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt) =>
        new()
        {
            Id = id,
            Name = name,
            Slug = slug,
            ParentId = parentId,
            SortOrder = sortOrder,
            Level = level,
            MaterializedPath = materializedPath,
            FullPath = fullPath,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

    public void Update(string name, string slug, int sortOrder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        Name = name.Trim();
        Slug = slug.Trim().ToLowerInvariant();
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
        ParentId = parentId;
        Level = level;
        MaterializedPath = materializedPath;
        FullPath = fullPath;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}