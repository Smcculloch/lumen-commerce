using Lumen.Domain.Templates;

namespace Lumen.Application.Templates;

/// <summary>
/// Supplies code-first template definitions compiled into the application.
/// </summary>
public interface ICodeTemplateProvider
{
    IReadOnlyList<TemplateDefinition> GetTemplates();
}