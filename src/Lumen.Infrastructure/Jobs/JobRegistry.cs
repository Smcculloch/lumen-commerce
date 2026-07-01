using Lumen.Application.Jobs.Dtos;

namespace Lumen.Infrastructure.Jobs;

public static class JobRegistry
{
    public static IReadOnlyList<JobDefinitionDto> Definitions { get; private set; } = [];

    public static void Initialize(IReadOnlyList<JobDefinitionDto> definitions) =>
        Definitions = definitions;
}