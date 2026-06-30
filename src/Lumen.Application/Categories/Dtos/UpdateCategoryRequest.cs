namespace Lumen.Application.Categories.Dtos;

public sealed record UpdateCategoryRequest(
    string Name,
    string? Slug,
    Guid? ParentId,
    int SortOrder);