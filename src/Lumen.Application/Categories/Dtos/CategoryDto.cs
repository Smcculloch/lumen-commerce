namespace Lumen.Application.Categories.Dtos;

public sealed record CategoryDto(
    Guid Id,
    string Name,
    string Slug,
    string FullPath,
    Guid? ParentId,
    int SortOrder,
    int Level,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);