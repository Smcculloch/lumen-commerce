namespace Lumen.Application.Content.Dtos;

/// <summary>
/// Request to create a new content item under an optional parent.
/// </summary>
public sealed record CreateContentRequest(
    string TemplateKey,
    string Name,
    string? Slug,
    Guid? ParentId,
    int SortOrder,
    IDictionary<string, object?> Properties);