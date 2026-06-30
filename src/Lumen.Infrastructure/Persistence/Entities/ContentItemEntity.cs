using Lumen.Domain.Enums;

namespace Lumen.Infrastructure.Persistence.Entities;

/// <summary>
/// EF persistence model for <see cref="Domain.Content.ContentItem"/>.
/// </summary>
public sealed class ContentItemEntity
{
    public Guid Id { get; set; }
    public string TemplateKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
    public int SortOrder { get; set; }
    public int Level { get; set; }
    public string MaterializedPath { get; set; } = "/";
    public string FullPath { get; set; } = "/";
    public ContentStatus Status { get; set; } = ContentStatus.Draft;
    public DateTimeOffset? PublishedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string PropertiesJson { get; set; } = "{}";

    public ContentItemEntity? Parent { get; set; }
    public ICollection<ContentItemEntity> Children { get; set; } = new List<ContentItemEntity>();
}