using Lumen.Domain.Enums;
using Lumen.Domain.Templates;
using Lumen.Domain.ValueObjects;

namespace Lumen.Application.Templates.Builders;

/// <summary>
/// Fluent builder for constructing template definitions in code.
/// </summary>
public sealed class TemplateBuilder<TTemplate> where TTemplate : TemplateDefinition
{
    private readonly string _key;
    private readonly string _displayName;
    private readonly TemplateKind _kind;
    private readonly List<PropertyDefinition> _properties = [];
    private string? _description;

    private TemplateBuilder(string key, string displayName, TemplateKind kind)
    {
        _key = key;
        _displayName = displayName;
        _kind = kind;
    }

    public static TemplateBuilder<ContentTemplateDefinition> ForContent(string key, string displayName) =>
        new(key, displayName, TemplateKind.Content);

    public static TemplateBuilder<ProductTemplateDefinition> ForProduct(string key, string displayName) =>
        new(key, displayName, TemplateKind.Product);

    public TemplateBuilder<TTemplate> WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public TemplateBuilder<TTemplate> AddProperty(
        string name,
        string displayName,
        PropertyType type,
        Action<PropertyBuilder>? configure = null)
    {
        var builder = new PropertyBuilder(name, displayName, type);
        configure?.Invoke(builder);
        _properties.Add(builder.Build());
        return this;
    }

    public TTemplate Build()
    {
        if (_kind == TemplateKind.Content)
        {
            return (TTemplate)(object)new ContentTemplateDefinition(_key, _displayName, _properties, _description);
        }

        return (TTemplate)(object)new ProductTemplateDefinition(_key, _displayName, _properties, _description);
    }
}

/// <summary>
/// Fluent configuration for a single property on a template builder.
/// </summary>
public sealed class PropertyBuilder
{
    private readonly string _name;
    private readonly string _displayName;
    private readonly PropertyType _type;
    private bool _isRequired;
    private int _sortOrder;
    private string? _defaultValue;
    private string? _description;
    private readonly List<SelectOption> _options = [];
    private PropertyValidationRules _validation = PropertyValidationRules.None;
    private string? _referenceTemplateKey;

    internal PropertyBuilder(string name, string displayName, PropertyType type)
    {
        _name = name;
        _displayName = displayName;
        _type = type;
    }

    public PropertyBuilder Required(bool required = true)
    {
        _isRequired = required;
        return this;
    }

    public PropertyBuilder SortOrder(int sortOrder)
    {
        _sortOrder = sortOrder;
        return this;
    }

    public PropertyBuilder DefaultValue(string? value)
    {
        _defaultValue = value;
        return this;
    }

    public PropertyBuilder Description(string description)
    {
        _description = description;
        return this;
    }

    public PropertyBuilder WithValidation(
        int? minLength = null,
        int? maxLength = null,
        decimal? minValue = null,
        decimal? maxValue = null,
        string? pattern = null)
    {
        _validation = new PropertyValidationRules(minLength, maxLength, minValue, maxValue, pattern);
        return this;
    }

    public PropertyBuilder MaxLength(int maxLength) => WithValidation(maxLength: maxLength);

    public PropertyBuilder MinValue(decimal minValue) => WithValidation(minValue: minValue);

    public PropertyBuilder ReferencesTemplate(string templateKey)
    {
        _referenceTemplateKey = templateKey;
        return this;
    }

    public PropertyBuilder AddOption(string value, string label, int sortOrder = 0)
    {
        _options.Add(new SelectOption(value, label, sortOrder));
        return this;
    }

    internal PropertyDefinition Build() =>
        new(_name, _displayName, _type, _isRequired, _sortOrder, _defaultValue, _description, _options, _validation,
            PropertySource.Code, _referenceTemplateKey);
}