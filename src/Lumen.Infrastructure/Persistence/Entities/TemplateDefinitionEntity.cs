using Lumen.Domain.Enums;

namespace Lumen.Infrastructure.Persistence.Entities;

/// <summary>
/// Persisted template metadata for dynamic templates and code-template extensions.
/// </summary>
public sealed class TemplateDefinitionEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Key { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TemplateKind Kind { get; set; }

    /// <summary>
    /// When set, properties on this record extend a code-defined template with the same key.
    /// </summary>
    public string? BaseTemplateKey { get; set; }

    /// <summary>
    /// True when the template exists only in the database (no code counterpart).
    /// </summary>
    public bool IsDynamic { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<PropertyDefinitionEntity> Properties { get; set; } = new List<PropertyDefinitionEntity>();
}