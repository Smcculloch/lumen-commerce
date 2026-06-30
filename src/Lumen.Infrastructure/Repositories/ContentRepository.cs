using Lumen.Application.Content;
using Lumen.Domain.Content;
using Lumen.Infrastructure.Persistence;
using Lumen.Infrastructure.Persistence.Mapping;
using Microsoft.EntityFrameworkCore;

namespace Lumen.Infrastructure.Repositories;

public sealed class ContentRepository : IContentRepository
{
    private readonly AppDbContext _dbContext;

    public ContentRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ContentItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.ContentItems.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return entity is null ? null : InstanceMapping.ToDomain(entity);
    }

    public async Task<ContentItem?> GetByFullPathAsync(string fullPath, CancellationToken cancellationToken = default)
    {
        var normalized = fullPath.Trim().ToLowerInvariant();
        var entity = await _dbContext.ContentItems.FirstOrDefaultAsync(x => x.FullPath == normalized, cancellationToken);
        return entity is null ? null : InstanceMapping.ToDomain(entity);
    }

    public async Task<IReadOnlyList<ContentItem>> GetChildrenAsync(Guid parentId, CancellationToken cancellationToken = default)
    {
        var entities = await _dbContext.ContentItems
            .Where(x => x.ParentId == parentId)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return entities.Select(InstanceMapping.ToDomain).ToList();
    }

    public async Task<IReadOnlyList<ContentItem>> GetDescendantsAsync(Guid contentId, CancellationToken cancellationToken = default)
    {
        var root = await _dbContext.ContentItems.FirstOrDefaultAsync(x => x.Id == contentId, cancellationToken);
        if (root is null)
        {
            return [];
        }

        var entities = await _dbContext.ContentItems
            .Where(x => x.MaterializedPath.StartsWith(root.MaterializedPath) && x.Id != contentId)
            .OrderBy(x => x.Level)
            .ThenBy(x => x.SortOrder)
            .ToListAsync(cancellationToken);

        return entities.Select(InstanceMapping.ToDomain).ToList();
    }

    public async Task<IReadOnlyList<ContentItem>> GetRootsAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _dbContext.ContentItems
            .Where(x => x.ParentId == null)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return entities.Select(InstanceMapping.ToDomain).ToList();
    }

    public async Task<IReadOnlyList<string>> GetSiblingSlugsAsync(Guid? parentId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ContentItems
            .Where(x => x.ParentId == parentId)
            .Select(x => x.Slug)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> HasChildrenAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.ContentItems.AnyAsync(x => x.ParentId == id, cancellationToken);

    public async Task AddAsync(ContentItem item, CancellationToken cancellationToken = default)
    {
        await _dbContext.ContentItems.AddAsync(InstanceMapping.ToEntity(item), cancellationToken);
    }

    public async Task UpdateAsync(ContentItem item, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.ContentItems.FindAsync([item.Id], cancellationToken)
            ?? throw new InvalidOperationException($"Content '{item.Id}' was not found.");

        _dbContext.Entry(entity).CurrentValues.SetValues(InstanceMapping.ToEntity(item));
    }

    public async Task DeleteAsync(ContentItem item, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.ContentItems.FindAsync([item.Id], cancellationToken);
        if (entity is not null)
        {
            _dbContext.ContentItems.Remove(entity);
        }
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}