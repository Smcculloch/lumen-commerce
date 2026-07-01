using Lumen.Application.Jobs;
using Lumen.Domain.Jobs;

namespace Lumen.Infrastructure.Jobs;

public sealed class JobExecutionLogger
{
    private readonly IJobExecutionRepository _repository;

    public JobExecutionLogger(IJobExecutionRepository repository)
    {
        _repository = repository;
    }

    public async Task<string> RunAsync(string jobKey, Func<Task<string>> work, CancellationToken cancellationToken)
    {
        var execution = JobExecution.Start(jobKey);

        try
        {
            var message = await work();
            execution.Complete(message);
            await _repository.AddAsync(execution, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            return message;
        }
        catch (Exception ex)
        {
            execution.Fail(ex.Message);
            await _repository.AddAsync(execution, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            throw;
        }
    }
}