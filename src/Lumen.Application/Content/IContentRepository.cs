using Lumen.Domain.Content;

namespace Lumen.Application.Content;

/// <summary>
/// Persistence abstraction for CMS content items and tree queries.
/// </summary>
public interface IContentRepository
{
    Task<ContentItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ContentItem?> GetByFullPathAsync(string fullPath, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ContentItem>> GetChildrenAsync(Guid parentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ContentItem>> GetDescendantsAsync(Guid contentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ContentItem>> GetRootsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetSiblingSlugsAsync(Guid? parentId, CancellationToken cancellationToken = default);
    Task<bool> HasChildrenAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(ContentItem item, CancellationToken cancellationToken = default);
    Task UpdateAsync(ContentItem item, CancellationToken cancellationToken = default);
    Task DeleteAsync(ContentItem item, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}