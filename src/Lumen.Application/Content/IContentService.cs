using Lumen.Application.Content.Dtos;

namespace Lumen.Application.Content;

/// <summary>
/// Core CMS operations for managing hierarchical content backed by templates.
/// </summary>
public interface IContentService
{
    Task<ContentDto?> GetContentByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ContentDto?> GetContentBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ContentDto>> GetRootsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NavigationItemDto>> GetNavigationAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ContentDto>> GetChildrenAsync(Guid parentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ContentDto>> GetDescendantsAsync(Guid contentId, CancellationToken cancellationToken = default);
    Task<ContentDto> CreateContentAsync(CreateContentRequest request, CancellationToken cancellationToken = default);
    Task<ContentDto> UpdateContentAsync(Guid id, UpdateContentRequest request, CancellationToken cancellationToken = default);
    Task<ContentDto> PublishContentAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ContentDto> UnpublishContentAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteContentAsync(Guid id, CancellationToken cancellationToken = default);
    Task ReorderSiblingsAsync(Guid? parentId, IReadOnlyList<Guid> orderedIds, CancellationToken cancellationToken = default);
}