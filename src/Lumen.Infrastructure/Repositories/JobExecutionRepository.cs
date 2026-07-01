using Lumen.Application.Jobs;
using Lumen.Domain.Jobs;
using Lumen.Infrastructure.Persistence;
using Lumen.Infrastructure.Persistence.Mapping;
using Microsoft.EntityFrameworkCore;

namespace Lumen.Infrastructure.Repositories;

public sealed class JobExecutionRepository : IJobExecutionRepository
{
    private readonly AppDbContext _dbContext;

    public JobExecutionRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(JobExecution execution, CancellationToken cancellationToken = default) =>
        await _dbContext.JobExecutions.AddAsync(JobMapping.ToEntity(execution), cancellationToken);

    public async Task<IReadOnlyList<JobExecution>> ListRecentAsync(int take, CancellationToken cancellationToken = default)
    {
        var entities = await _dbContext.JobExecutions.ToListAsync(cancellationToken);

        return entities
            .OrderByDescending(x => x.StartedAt)
            .Take(take)
            .Select(JobMapping.ToDomain)
            .ToList();
    }

    public async Task<JobExecution?> GetLatestForJobAsync(string jobKey, CancellationToken cancellationToken = default)
    {
        var entities = await _dbContext.JobExecutions
            .Where(x => x.JobKey == jobKey)
            .ToListAsync(cancellationToken);

        var entity = entities
            .OrderByDescending(x => x.StartedAt)
            .FirstOrDefault();

        return entity is null ? null : JobMapping.ToDomain(entity);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}