using Lumen.Domain.Enums;
using Lumen.Domain.Templates;

namespace Lumen.Application.Templates;

/// <summary>
/// Discovers and resolves template definitions from code and persistent storage.
/// </summary>
public interface ITemplateRegistry
{
    Task<ResolvedTemplateDefinition?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ResolvedTemplateDefinition>> GetAllAsync(
        TemplateKind? kind = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ResolvedTemplateDefinition>> GetCodeDefinedAsync(
        TemplateKind? kind = null,
        CancellationToken cancellationToken = default);
}