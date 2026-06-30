using System.Reflection;
using Lumen.Application.Templates.Definitions;
using Lumen.Application.Templates.Discovery;
using Lumen.Domain.Templates;

namespace Lumen.Application.Templates;

/// <summary>
/// Aggregates all code-first templates from attribute discovery and explicit fluent definitions.
/// </summary>
public sealed class LumenCodeTemplateProvider : ICodeTemplateProvider
{
    private readonly Lazy<IReadOnlyList<TemplateDefinition>> _templates = new(LoadTemplates);

    public IReadOnlyList<TemplateDefinition> GetTemplates() => _templates.Value;

    private static IReadOnlyList<TemplateDefinition> LoadTemplates()
    {
        var assembly = typeof(LumenCodeTemplateProvider).Assembly;
        var attributeTemplates = AttributeTemplateDiscovery.DiscoverFromAssembly(assembly);
        var fluentTemplates = new TemplateDefinition[]
        {
            StandardProductTemplate.Create()
        };

        return attributeTemplates
            .Concat(fluentTemplates)
            .GroupBy(t => t.Key, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();
    }
}