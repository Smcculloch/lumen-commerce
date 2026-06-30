using Lumen.Domain.Media;

namespace Lumen.Application.Media;

/// <summary>
/// Persistence abstraction for media library items.
/// </summary>
public interface IMediaRepository
{
    Task<MediaItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MediaItem>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(MediaItem item, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}