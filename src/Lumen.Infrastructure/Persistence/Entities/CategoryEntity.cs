namespace Lumen.Infrastructure.Persistence.Entities;

/// <summary>
/// EF persistence model for <see cref="Domain.Categories.Category"/>.
/// </summary>
public sealed class CategoryEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
    public int SortOrder { get; set; }
    public int Level { get; set; }
    public string MaterializedPath { get; set; } = "/";
    public string FullPath { get; set; } = "/";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public CategoryEntity? Parent { get; set; }
    public ICollection<CategoryEntity> Children { get; set; } = new List<CategoryEntity>();
    public ICollection<ProductEntity> Products { get; set; } = new List<ProductEntity>();
}