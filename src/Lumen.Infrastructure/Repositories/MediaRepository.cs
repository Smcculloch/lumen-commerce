using Lumen.Application.Media;
using Lumen.Domain.Media;
using Lumen.Infrastructure.Persistence;
using Lumen.Infrastructure.Persistence.Mapping;
using Microsoft.EntityFrameworkCore;

namespace Lumen.Infrastructure.Repositories;

public sealed class MediaRepository : IMediaRepository
{
    private readonly AppDbContext _dbContext;

    public MediaRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<MediaItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.MediaItems.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return entity is null ? null : InstanceMapping.ToDomain(entity);
    }

    public async Task<IReadOnlyList<MediaItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _dbContext.MediaItems.ToListAsync(cancellationToken);

        return entities
            .OrderByDescending(x => x.CreatedAt)
            .Select(InstanceMapping.ToDomain)
            .ToList();
    }

    public async Task AddAsync(MediaItem item, CancellationToken cancellationToken = default)
    {
        await _dbContext.MediaItems.AddAsync(InstanceMapping.ToEntity(item), cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}