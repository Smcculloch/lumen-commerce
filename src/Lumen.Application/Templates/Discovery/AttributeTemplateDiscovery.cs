using System.Reflection;
using Lumen.Application.Templates.Attributes;
using Lumen.Domain.Enums;
using Lumen.Domain.Templates;
using Lumen.Domain.ValueObjects;

namespace Lumen.Application.Templates.Discovery;

/// <summary>
/// Discovers code-first templates declared with <see cref="ContentTemplateAttribute"/>
/// and <see cref="ProductTemplateAttribute"/> on POCO classes.
/// </summary>
public static class AttributeTemplateDiscovery
{
    public static IReadOnlyList<TemplateDefinition> DiscoverFromAssembly(Assembly assembly)
    {
        var templates = new List<TemplateDefinition>();

        foreach (var type in assembly.GetTypes().Where(t => t is { IsClass: true, IsAbstract: false }))
        {
            var contentAttribute = type.GetCustomAttribute<ContentTemplateAttribute>();
            if (contentAttribute is not null)
            {
                templates.Add(new ContentTemplateDefinition(
                    contentAttribute.Key,
                    contentAttribute.DisplayName,
                    DiscoverProperties(type),
                    contentAttribute.Description));
                continue;
            }

            var productAttribute = type.GetCustomAttribute<ProductTemplateAttribute>();
            if (productAttribute is not null)
            {
                templates.Add(new ProductTemplateDefinition(
                    productAttribute.Key,
                    productAttribute.DisplayName,
                    DiscoverProperties(type),
                    productAttribute.Description));
            }
        }

        return templates;
    }

    private static IReadOnlyList<PropertyDefinition> DiscoverProperties(Type templateType)
    {
        return templateType
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select((property, index) => (Property: property, Attribute: property.GetCustomAttribute<TemplatePropertyAttribute>(), Index: index))
            .Where(x => x.Attribute is not null)
            .Select(x => new PropertyDefinition(
                ToCamelCase(x.Property.Name),
                x.Attribute!.DisplayName ?? x.Property.Name,
                x.Attribute.Type,
                x.Attribute.IsRequired,
                x.Attribute.SortOrder == 0 ? (x.Index + 1) * 10 : x.Attribute.SortOrder,
                x.Attribute.DefaultValue,
                x.Attribute.Description,
                ParseSelectOptions(x.Attribute.SelectOptions),
                new PropertyValidationRules(
                    x.Attribute.MinLength >= 0 ? x.Attribute.MinLength : null,
                    x.Attribute.MaxLength >= 0 ? x.Attribute.MaxLength : null,
                    x.Attribute.MinValue > decimal.MinValue ? x.Attribute.MinValue : null,
                    x.Attribute.MaxValue < decimal.MaxValue ? x.Attribute.MaxValue : null,
                    x.Attribute.Pattern),
                PropertySource.Code,
                x.Attribute.ReferenceTemplateKey))
            .OrderBy(p => p.SortOrder)
            .ToList();
    }

    private static IReadOnlyList<SelectOption> ParseSelectOptions(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return Array.Empty<SelectOption>();
        }

        return raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select((entry, index) =>
            {
                var parts = entry.Split(':', 2, StringSplitOptions.TrimEntries);
                return parts.Length == 2
                    ? new SelectOption(parts[0], parts[1], index)
                    : new SelectOption(parts[0], parts[0], index);
            })
            .ToList();
    }

    private static string ToCamelCase(string value) =>
        string.IsNullOrEmpty(value) ? value : char.ToLowerInvariant(value[0]) + value[1..];
}