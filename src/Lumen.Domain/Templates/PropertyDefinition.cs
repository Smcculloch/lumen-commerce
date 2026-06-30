using Lumen.Domain.Enums;
using Lumen.Domain.ValueObjects;

namespace Lumen.Domain.Templates;

/// <summary>
/// Describes a single field on a content or product template.
/// </summary>
public sealed class PropertyDefinition
{
    public PropertyDefinition(
        string name,
        string displayName,
        PropertyType type,
        bool isRequired = false,
        int sortOrder = 0,
        string? defaultValue = null,
        string? description = null,
        IReadOnlyList<SelectOption>? options = null,
        PropertyValidationRules? validation = null,
        PropertySource source = PropertySource.Code,
        string? referenceTemplateKey = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        Name = name.Trim();
        DisplayName = displayName.Trim();
        Type = type;
        IsRequired = isRequired;
        SortOrder = sortOrder;
        DefaultValue = defaultValue;
        Description = description;
        Options = options ?? Array.Empty<SelectOption>();
        Validation = validation ?? PropertyValidationRules.None;
        Source = source;
        ReferenceTemplateKey = referenceTemplateKey;
    }

    public string Name { get; }
    public string DisplayName { get; }
    public string? Description { get; }
    public PropertyType Type { get; }
    public bool IsRequired { get; }
    public int SortOrder { get; }
    public string? DefaultValue { get; }
    public IReadOnlyList<SelectOption> Options { get; }
    public PropertyValidationRules Validation { get; }
    public PropertySource Source { get; }
    public string? ReferenceTemplateKey { get; }
}