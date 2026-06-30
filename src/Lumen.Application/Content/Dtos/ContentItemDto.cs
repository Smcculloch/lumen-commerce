namespace Lumen.Application.Content.Dtos;

/// <summary>
/// Application-layer representation of a CMS content instance.
/// </summary>
public sealed record ContentItemDto(
    Guid Id,
    string TemplateKey,
    string Name,
    string? Slug,
    bool IsPublished,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyDictionary<string, object?> Properties);