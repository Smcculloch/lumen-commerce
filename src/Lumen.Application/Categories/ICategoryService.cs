using Lumen.Application.Categories.Dtos;

namespace Lumen.Application.Categories;

public interface ICategoryService
{
    Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CategoryDto>> GetRootsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CategoryDto>> GetChildrenAsync(Guid parentId, CancellationToken cancellationToken = default);
    Task<CategoryDto> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default);
    Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task ReorderSiblingsAsync(Guid? parentId, IReadOnlyList<Guid> orderedIds, CancellationToken cancellationToken = default);
}