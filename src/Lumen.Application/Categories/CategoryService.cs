using Lumen.Application.Categories.Dtos;
using Lumen.Domain.Categories;

namespace Lumen.Application.Categories;

public sealed class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;

    public CategoryService(ICategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _repository.GetByIdAsync(id, cancellationToken);
        return category is null ? null : Map(category);
    }

    public async Task<IReadOnlyList<CategoryDto>> GetRootsAsync(CancellationToken cancellationToken = default)
    {
        var roots = await _repository.GetRootsAsync(cancellationToken);
        return roots.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<CategoryDto>> GetChildrenAsync(Guid parentId, CancellationToken cancellationToken = default)
    {
        var children = await _repository.GetChildrenAsync(parentId, cancellationToken);
        return children.Select(Map).ToList();
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var parent = await ResolveParentAsync(request.ParentId, cancellationToken);
        var slug = await ResolveSlugAsync(request.Slug, request.Name, request.ParentId, cancellationToken);

        var category = Category.Create(
            request.Name,
            slug,
            request.ParentId,
            request.SortOrder,
            CategoryPathHelper.BuildLevel(parent),
            materializedPath: "/",
            fullPath: CategoryPathHelper.BuildFullPath(slug, parent));

        category = Category.Rehydrate(
            category.Id,
            category.Name,
            category.Slug,
            category.ParentId,
            category.SortOrder,
            category.Level,
            CategoryPathHelper.BuildMaterializedPath(category.Id, parent),
            category.FullPath,
            category.CreatedAt,
            category.UpdatedAt);

        await _repository.AddAsync(category, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return Map(category);
    }

    public async Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException($"Category '{id}' was not found.");

        if (request.ParentId == category.Id || IsDescendantParent(category, request.ParentId))
        {
            throw new InvalidOperationException("A category cannot be moved under itself or its descendants.");
        }

        var parent = request.ParentId == category.ParentId
            ? await ResolveParentAsync(category.ParentId, cancellationToken)
            : await ResolveParentAsync(request.ParentId, cancellationToken);

        var slug = await ResolveSlugAsync(request.Slug ?? category.Slug, request.Name, request.ParentId, cancellationToken, excludeId: id);
        category.Update(request.Name, slug, request.SortOrder);
        category.SetHierarchy(
            request.ParentId,
            CategoryPathHelper.BuildLevel(parent),
            CategoryPathHelper.BuildMaterializedPath(category.Id, parent),
            CategoryPathHelper.BuildFullPath(slug, parent));

        await _repository.UpdateAsync(category, cancellationToken);

        var descendants = await _repository.GetDescendantsAsync(category.Id, cancellationToken);
        foreach (var descendant in descendants)
        {
            await RecomputeDescendantPathsAsync(descendant, cancellationToken);
            await _repository.UpdateAsync(descendant, cancellationToken);
        }

        await _repository.SaveChangesAsync(cancellationToken);
        return Map(category);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException($"Category '{id}' was not found.");

        if (await _repository.HasChildrenAsync(id, cancellationToken))
        {
            throw new InvalidOperationException("Cannot delete a category that still has child categories.");
        }

        await _repository.DeleteAsync(category, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    private async Task RecomputeDescendantPathsAsync(Category descendant, CancellationToken cancellationToken)
    {
        var parent = descendant.ParentId is Guid parentId
            ? await _repository.GetByIdAsync(parentId, cancellationToken)
            : null;

        if (parent is null)
        {
            throw new InvalidOperationException($"Parent '{descendant.ParentId}' was not found while recomputing paths.");
        }

        descendant.SetHierarchy(
            descendant.ParentId,
            CategoryPathHelper.BuildLevel(parent),
            CategoryPathHelper.BuildMaterializedPath(descendant.Id, parent),
            CategoryPathHelper.BuildFullPath(descendant.Slug, parent));
    }

    private static bool IsDescendantParent(Category category, Guid? newParentId) =>
        newParentId is Guid parentId && parentId != category.ParentId &&
        category.MaterializedPath.Contains($"/{parentId}/", StringComparison.Ordinal);

    private async Task<string> ResolveSlugAsync(
        string? requestedSlug,
        string name,
        Guid? parentId,
        CancellationToken cancellationToken,
        Guid? excludeId = null)
    {
        var baseSlug = string.IsNullOrWhiteSpace(requestedSlug)
            ? CategoryPathHelper.GenerateSlug(name)
            : requestedSlug.Trim().ToLowerInvariant();

        var siblings = await _repository.GetSiblingSlugsAsync(parentId, cancellationToken);
        if (excludeId is Guid excluded)
        {
            var current = await _repository.GetByIdAsync(excluded, cancellationToken);
            if (current is not null)
            {
                siblings = siblings.Where(s => !string.Equals(s, current.Slug, StringComparison.OrdinalIgnoreCase)).ToList();
            }
        }

        return CategoryPathHelper.EnsureUniqueSlug(baseSlug, siblings);
    }

    private async Task<Category?> ResolveParentAsync(Guid? parentId, CancellationToken cancellationToken)
    {
        if (parentId is null)
        {
            return null;
        }

        return await _repository.GetByIdAsync(parentId.Value, cancellationToken)
            ?? throw new InvalidOperationException($"Parent category '{parentId}' was not found.");
    }

    private static CategoryDto Map(Category category) =>
        new(
            category.Id,
            category.Name,
            category.Slug,
            category.FullPath,
            category.ParentId,
            category.SortOrder,
            category.Level,
            category.CreatedAt,
            category.UpdatedAt);
}