using Lumen.Domain.Enums;

namespace Lumen.Domain.Templates;

/// <summary>
/// Base definition describing the shape of a content or product template.
/// </summary>
public abstract class TemplateDefinition
{
    protected TemplateDefinition(
        string key,
        string displayName,
        TemplateKind kind,
        IReadOnlyList<PropertyDefinition> properties,
        string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        Key = key.Trim().ToLowerInvariant();
        DisplayName = displayName.Trim();
        Kind = kind;
        Properties = properties;
        Description = description;
    }

    public string Key { get; }
    public string DisplayName { get; }
    public string? Description { get; }
    public TemplateKind Kind { get; }
    public IReadOnlyList<PropertyDefinition> Properties { get; }
}

/// <summary>
/// A CMS content template definition.
/// </summary>
public sealed class ContentTemplateDefinition : TemplateDefinition
{
    public ContentTemplateDefinition(
        string key,
        string displayName,
        IReadOnlyList<PropertyDefinition> properties,
        string? description = null)
        : base(key, displayName, TemplateKind.Content, properties, description)
    {
    }
}

/// <summary>
/// A PIM product template definition.
/// </summary>
public sealed class ProductTemplateDefinition : TemplateDefinition
{
    public ProductTemplateDefinition(
        string key,
        string displayName,
        IReadOnlyList<PropertyDefinition> properties,
        string? description = null)
        : base(key, displayName, TemplateKind.Product, properties, description)
    {
    }
}

/// <summary>
/// A resolved template after merging code-defined and database-defined properties.
/// </summary>
public sealed class ResolvedTemplateDefinition
{
    public ResolvedTemplateDefinition(
        string key,
        string displayName,
        TemplateKind kind,
        IReadOnlyList<PropertyDefinition> properties,
        string? description = null,
        string? baseTemplateKey = null,
        bool isDynamic = false)
    {
        Key = key;
        DisplayName = displayName;
        Kind = kind;
        Properties = properties;
        Description = description;
        BaseTemplateKey = baseTemplateKey;
        IsDynamic = isDynamic;
    }

    public string Key { get; }
    public string DisplayName { get; }
    public string? Description { get; }
    public TemplateKind Kind { get; }
    public IReadOnlyList<PropertyDefinition> Properties { get; }
    public string? BaseTemplateKey { get; }
    public bool IsDynamic { get; }

    public PropertyDefinition? FindProperty(string name) =>
        Properties.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
}