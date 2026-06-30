using Lumen.Domain.Enums;

namespace Lumen.Infrastructure.Persistence.Entities;

/// <summary>
/// Persisted property definition for dynamic templates or template extensions.
/// </summary>
public sealed class PropertyDefinitionEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TemplateDefinitionId { get; set; }
    public TemplateDefinitionEntity TemplateDefinition { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public PropertyType Type { get; set; }
    public bool IsRequired { get; set; }
    public int SortOrder { get; set; }
    public string? DefaultValue { get; set; }
    public string? ReferenceTemplateKey { get; set; }
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public string? Pattern { get; set; }

    /// <summary>
    /// JSON array of { value, label, sortOrder } for select fields.
    /// </summary>
    public string? OptionsJson { get; set; }
}