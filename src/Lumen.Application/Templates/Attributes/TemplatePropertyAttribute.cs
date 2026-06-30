using Lumen.Domain.Enums;

namespace Lumen.Application.Templates.Attributes;

/// <summary>
/// Declares a strongly-typed property on an attribute-driven template class.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class TemplatePropertyAttribute : Attribute
{
    public TemplatePropertyAttribute(PropertyType type)
    {
        Type = type;
    }

    public PropertyType Type { get; }
    public string? DisplayName { get; init; }
    public string? Description { get; init; }
    public bool IsRequired { get; init; }
    public int SortOrder { get; init; }
    public string? DefaultValue { get; init; }
    /// <summary>Use -1 to indicate no minimum length constraint.</summary>
    public int MinLength { get; init; } = -1;

    /// <summary>Use -1 to indicate no maximum length constraint.</summary>
    public int MaxLength { get; init; } = -1;

    /// <summary>Use <see cref="decimal.MinValue"/> to indicate no minimum value constraint.</summary>
    public decimal MinValue { get; init; } = decimal.MinValue;

    /// <summary>Use <see cref="decimal.MaxValue"/> to indicate no maximum value constraint.</summary>
    public decimal MaxValue { get; init; } = decimal.MaxValue;
    public string? Pattern { get; init; }
    public string? ReferenceTemplateKey { get; init; }
    public string? SelectOptions { get; init; }
}