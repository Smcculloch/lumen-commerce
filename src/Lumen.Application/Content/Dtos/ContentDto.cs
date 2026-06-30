using Lumen.Domain.Enums;

namespace Lumen.Application.Content.Dtos;

/// <summary>
/// Application-layer representation of a CMS content item in the tree.
/// </summary>
public sealed record ContentDto(
    Guid Id,
    string TemplateKey,
    string Name,
    string Slug,
    string FullPath,
    Guid? ParentId,
    int SortOrder,
    int Level,
    ContentStatus Status,
    DateTimeOffset? PublishedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyDictionary<string, object?> Properties);