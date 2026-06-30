namespace Lumen.Application.Content.Dtos;

/// <summary>
/// Request to create a new content item from a template.
/// </summary>
public sealed record CreateContentItemRequest(
    string TemplateKey,
    string Name,
    string? Slug,
    IDictionary<string, object?> Properties);