using Lumen.Application.Jobs.Dtos;

namespace Lumen.Application.Jobs;

public interface IJobRunner
{
    IReadOnlyList<JobDefinitionDto> ListJobs();
    Task<IReadOnlyList<JobExecutionDto>> ListRecentExecutionsAsync(int take = 20, CancellationToken cancellationToken = default);
    Task<JobExecutionDto?> GetLatestExecutionAsync(string jobKey, CancellationToken cancellationToken = default);
    Task<JobTriggerResult> TriggerAsync(string jobKey, CancellationToken cancellationToken = default);
}