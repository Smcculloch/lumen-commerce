using Lumen.Domain.Jobs;

namespace Lumen.Application.Jobs;

public interface IJobExecutionRepository
{
    Task AddAsync(JobExecution execution, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<JobExecution>> ListRecentAsync(int take, CancellationToken cancellationToken = default);
    Task<JobExecution?> GetLatestForJobAsync(string jobKey, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}