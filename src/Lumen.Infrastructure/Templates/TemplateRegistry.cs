using Lumen.Application.Templates;
using Lumen.Domain.Enums;
using Lumen.Domain.Templates;
using Lumen.Infrastructure.Persistence;
using Lumen.Infrastructure.Persistence.Mapping;
using Microsoft.EntityFrameworkCore;

namespace Lumen.Infrastructure.Templates;

/// <summary>
/// Resolves templates by merging code-first definitions with database extensions and dynamic templates.
/// </summary>
public sealed class TemplateRegistry : ITemplateRegistry
{
    private readonly AppDbContext _dbContext;
    private readonly ICodeTemplateProvider _codeTemplateProvider;

    public TemplateRegistry(AppDbContext dbContext, ICodeTemplateProvider codeTemplateProvider)
    {
        _dbContext = dbContext;
        _codeTemplateProvider = codeTemplateProvider;
    }

    public async Task<ResolvedTemplateDefinition?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        var normalizedKey = key.Trim().ToLowerInvariant();
        var codeTemplate = _codeTemplateProvider.GetTemplates()
            .FirstOrDefault(t => string.Equals(t.Key, normalizedKey, StringComparison.OrdinalIgnoreCase));

        var persistedTemplate = await _dbContext.TemplateDefinitions
            .Include(t => t.Properties)
            .FirstOrDefaultAsync(t => t.Key == normalizedKey, cancellationToken);

        if (codeTemplate is null && persistedTemplate is null)
        {
            return null;
        }

        return TemplateMapping.Merge(codeTemplate, persistedTemplate);
    }

    public async Task<IReadOnlyList<ResolvedTemplateDefinition>> GetAllAsync(
        TemplateKind? kind = null,
        CancellationToken cancellationToken = default)
    {
        var codeTemplates = _codeTemplateProvider.GetTemplates();
        if (kind.HasValue)
        {
            codeTemplates = codeTemplates.Where(t => t.Kind == kind.Value).ToList();
        }

        var persistedTemplates = await _dbContext.TemplateDefinitions
            .Include(t => t.Properties)
            .ToListAsync(cancellationToken);

        if (kind.HasValue)
        {
            persistedTemplates = persistedTemplates.Where(t => t.Kind == kind.Value).ToList();
        }

        var keys = codeTemplates.Select(t => t.Key)
            .Concat(persistedTemplates.Select(t => t.Key))
            .Distinct(StringComparer.OrdinalIgnoreCase);

        var resolved = new List<ResolvedTemplateDefinition>();
        foreach (var templateKey in keys)
        {
            var codeTemplate = codeTemplates.FirstOrDefault(t =>
                string.Equals(t.Key, templateKey, StringComparison.OrdinalIgnoreCase));
            var persistedTemplate = persistedTemplates.FirstOrDefault(t =>
                string.Equals(t.Key, templateKey, StringComparison.OrdinalIgnoreCase));

            resolved.Add(TemplateMapping.Merge(codeTemplate, persistedTemplate));
        }

        return resolved.OrderBy(t => t.DisplayName).ToList();
    }

    public Task<IReadOnlyList<ResolvedTemplateDefinition>> GetCodeDefinedAsync(
        TemplateKind? kind = null,
        CancellationToken cancellationToken = default)
    {
        var templates = _codeTemplateProvider.GetTemplates();
        if (kind.HasValue)
        {
            templates = templates.Where(t => t.Kind == kind.Value).ToList();
        }

        IReadOnlyList<ResolvedTemplateDefinition> resolved = templates
            .Select(t => new ResolvedTemplateDefinition(t.Key, t.DisplayName, t.Kind, t.Properties, t.Description))
            .OrderBy(t => t.DisplayName)
            .ToList();

        return Task.FromResult(resolved);
    }
}