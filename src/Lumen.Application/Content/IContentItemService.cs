using Lumen.Application.Content.Dtos;

namespace Lumen.Application.Content;

/// <summary>
/// Creates and validates CMS content instances using resolved templates.
/// </summary>
public interface IContentItemService
{
    Task<ContentItemDto> CreateAsync(CreateContentItemRequest request, CancellationToken cancellationToken = default);
    Task<ContentItemDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}