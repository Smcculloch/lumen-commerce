using System.Text.Json;
using Lumen.Application.Templates.Management.Dtos;
using Lumen.Domain.Enums;
using Lumen.Domain.Templates;
using Lumen.Domain.ValueObjects;
using Lumen.Infrastructure.Persistence.Entities;

namespace Lumen.Infrastructure.Persistence.Mapping;

internal static class TemplateMapping
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static PropertyDefinition ToDomain(PropertyDefinitionEntity entity, PropertySource source) =>
        new(
            entity.Name,
            entity.DisplayName,
            entity.Type,
            entity.IsRequired,
            entity.SortOrder,
            entity.DefaultValue,
            entity.Description,
            DeserializeOptions(entity.OptionsJson),
            new PropertyValidationRules(entity.MinLength, entity.MaxLength, entity.MinValue, entity.MaxValue, entity.Pattern),
            source,
            entity.ReferenceTemplateKey);

    public static ResolvedTemplateDefinition Merge(
        TemplateDefinition? codeTemplate,
        TemplateDefinitionEntity? persistedTemplate)
    {
        if (codeTemplate is null && persistedTemplate is null)
        {
            throw new InvalidOperationException("Cannot merge templates when both sources are null.");
        }

        if (codeTemplate is not null && persistedTemplate is null)
        {
            return new ResolvedTemplateDefinition(
                codeTemplate.Key,
                codeTemplate.DisplayName,
                codeTemplate.Kind,
                codeTemplate.Properties,
                codeTemplate.Description);
        }

        if (codeTemplate is null && persistedTemplate is not null)
        {
            var dynamicProperties = persistedTemplate.Properties
                .OrderBy(p => p.SortOrder)
                .Select(p => ToDomain(p, PropertySource.Dynamic))
                .ToList();

            return new ResolvedTemplateDefinition(
                persistedTemplate.Key,
                persistedTemplate.DisplayName,
                persistedTemplate.Kind,
                dynamicProperties,
                persistedTemplate.Description,
                baseTemplateKey: persistedTemplate.BaseTemplateKey,
                isDynamic: persistedTemplate.IsDynamic);
        }

        var extensionProperties = persistedTemplate!.Properties
            .OrderBy(p => p.SortOrder)
            .Select(p => ToDomain(p, PropertySource.Extension))
            .ToList();

        var mergedProperties = codeTemplate!.Properties
            .Concat(extensionProperties)
            .GroupBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.OrderBy(p => p.Source).First())
            .OrderBy(p => p.SortOrder)
            .ToList();

        return new ResolvedTemplateDefinition(
            codeTemplate.Key,
            codeTemplate.DisplayName,
            codeTemplate.Kind,
            mergedProperties,
            codeTemplate.Description,
            baseTemplateKey: codeTemplate.Key,
            isDynamic: false);
    }

    public static PersistedTemplateDto ToDto(TemplateDefinitionEntity entity) =>
        new(
            entity.Id,
            entity.Key,
            entity.DisplayName,
            entity.Description,
            entity.Kind,
            entity.BaseTemplateKey,
            entity.IsDynamic);

    public static PersistedPropertyDto ToDto(PropertyDefinitionEntity entity) =>
        new(
            entity.Id,
            entity.TemplateDefinitionId,
            entity.Name,
            entity.DisplayName,
            entity.Description,
            entity.Type,
            entity.IsRequired,
            entity.SortOrder,
            entity.DefaultValue,
            entity.ReferenceTemplateKey,
            entity.MinLength,
            entity.MaxLength,
            entity.MinValue,
            entity.MaxValue,
            entity.Pattern,
            DeserializeOptions(entity.OptionsJson)
                .Select(o => new SelectOptionDto(o.Value, o.Label, o.SortOrder))
                .ToList());

    public static PropertyDefinitionEntity ToEntity(Guid templateId, CreatePropertyPersistenceRequest request) =>
        new()
        {
            TemplateDefinitionId = templateId,
            Name = request.Name,
            DisplayName = request.DisplayName,
            Description = request.Description,
            Type = request.Type,
            IsRequired = request.IsRequired,
            SortOrder = request.SortOrder,
            DefaultValue = request.DefaultValue,
            MinLength = request.MinLength,
            MaxLength = request.MaxLength,
            MinValue = request.MinValue,
            MaxValue = request.MaxValue,
            Pattern = request.Pattern,
            OptionsJson = SerializeOptions(request.Options)
        };

    public static void ApplyUpdate(PropertyDefinitionEntity entity, UpdatePropertyPersistenceRequest request)
    {
        entity.DisplayName = request.DisplayName;
        entity.Description = request.Description;
        entity.IsRequired = request.IsRequired;
        entity.SortOrder = request.SortOrder;
        entity.DefaultValue = request.DefaultValue;
        entity.MinLength = request.MinLength;
        entity.MaxLength = request.MaxLength;
        entity.MinValue = request.MinValue;
        entity.MaxValue = request.MaxValue;
        entity.Pattern = request.Pattern;
        entity.OptionsJson = SerializeOptions(request.Options);
    }

    private static string? SerializeOptions(IReadOnlyList<SelectOptionDto>? options)
    {
        if (options is null || options.Count == 0)
        {
            return null;
        }

        return JsonSerializer.Serialize(
            options.Select(o => new { value = o.Value, label = o.Label, sortOrder = o.SortOrder }),
            JsonOptions);
    }

    private static IReadOnlyList<SelectOption> DeserializeOptions(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<SelectOption>();
        }

        return JsonSerializer.Deserialize<List<SelectOption>>(json, JsonOptions) ?? [];
    }
}