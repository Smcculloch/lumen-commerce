namespace Lumen.Application.Content.Dtos;

/// <summary>
/// Request to update an existing content item.
/// </summary>
public sealed record UpdateContentRequest(
    string Name,
    string? Slug,
    Guid? ParentId,
    int? SortOrder,
    IDictionary<string, object?> Properties);