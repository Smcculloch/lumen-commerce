using Lumen.Application.Content.Dtos;

namespace Lumen.Application.Content;

/// <summary>
/// Backward-compatible facade over <see cref="IContentService"/> from Phase 1.
/// </summary>
public sealed class ContentItemService : IContentItemService
{
    private readonly IContentService _contentService;

    public ContentItemService(IContentService contentService)
    {
        _contentService = contentService;
    }

    public async Task<ContentItemDto> CreateAsync(
        CreateContentItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var created = await _contentService.CreateContentAsync(
            new CreateContentRequest(
                request.TemplateKey,
                request.Name,
                request.Slug,
                ParentId: null,
                SortOrder: 0,
                request.Properties),
            cancellationToken);

        return Map(created);
    }

    public async Task<ContentItemDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await _contentService.GetContentByIdAsync(id, cancellationToken);
        return item is null ? null : Map(item);
    }

    private static ContentItemDto Map(ContentDto item) =>
        new(
            item.Id,
            item.TemplateKey,
            item.Name,
            item.Slug,
            item.Status == Domain.Enums.ContentStatus.Published,
            item.CreatedAt,
            item.UpdatedAt,
            item.Properties);
}