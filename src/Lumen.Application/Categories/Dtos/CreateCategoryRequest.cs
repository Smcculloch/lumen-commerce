namespace Lumen.Application.Categories.Dtos;

public sealed record CreateCategoryRequest(
    string Name,
    string? Slug,
    Guid? ParentId,
    int SortOrder = 0);