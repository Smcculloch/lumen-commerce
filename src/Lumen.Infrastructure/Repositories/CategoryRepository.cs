using Lumen.Application.Categories;
using Lumen.Domain.Categories;
using Lumen.Infrastructure.Persistence;
using Lumen.Infrastructure.Persistence.Mapping;
using Microsoft.EntityFrameworkCore;

namespace Lumen.Infrastructure.Repositories;

public sealed class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _dbContext;

    public CategoryRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Categories.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return entity is null ? null : InstanceMapping.ToDomain(entity);
    }

    public async Task<IReadOnlyList<Category>> GetRootsAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _dbContext.Categories
            .Where(x => x.ParentId == null)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return entities.Select(InstanceMapping.ToDomain).ToList();
    }

    public async Task<IReadOnlyList<Category>> GetChildrenAsync(Guid parentId, CancellationToken cancellationToken = default)
    {
        var entities = await _dbContext.Categories
            .Where(x => x.ParentId == parentId)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return entities.Select(InstanceMapping.ToDomain).ToList();
    }

    public async Task<IReadOnlyList<Category>> GetDescendantsAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        var root = await _dbContext.Categories.FirstOrDefaultAsync(x => x.Id == categoryId, cancellationToken);
        if (root is null)
        {
            return [];
        }

        var entities = await _dbContext.Categories
            .Where(x => x.MaterializedPath.StartsWith(root.MaterializedPath) && x.Id != categoryId)
            .OrderBy(x => x.Level)
            .ThenBy(x => x.SortOrder)
            .ToListAsync(cancellationToken);

        return entities.Select(InstanceMapping.ToDomain).ToList();
    }

    public async Task<IReadOnlyList<string>> GetSiblingSlugsAsync(Guid? parentId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Categories
            .Where(x => x.ParentId == parentId)
            .Select(x => x.Slug)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> HasChildrenAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Categories.AnyAsync(x => x.ParentId == id, cancellationToken);

    public async Task AddAsync(Category category, CancellationToken cancellationToken = default)
    {
        await _dbContext.Categories.AddAsync(InstanceMapping.ToEntity(category), cancellationToken);
    }

    public async Task UpdateAsync(Category category, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Categories.FindAsync([category.Id], cancellationToken)
            ?? throw new InvalidOperationException($"Category '{category.Id}' was not found.");

        _dbContext.Entry(entity).CurrentValues.SetValues(InstanceMapping.ToEntity(category));
    }

    public async Task DeleteAsync(Category category, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Categories.FindAsync([category.Id], cancellationToken);
        if (entity is not null)
        {
            _dbContext.Categories.Remove(entity);
        }
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}