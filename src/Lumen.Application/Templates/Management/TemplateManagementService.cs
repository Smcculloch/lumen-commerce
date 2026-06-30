using Lumen.Application.Templates.Management.Dtos;
using Lumen.Domain.Enums;
using Lumen.Domain.Templates;

namespace Lumen.Application.Templates.Management;

public sealed class TemplateManagementService : ITemplateManagementService
{
    private readonly ITemplateRegistry _registry;
    private readonly ITemplateRepository _repository;
    private readonly ICodeTemplateProvider _codeTemplateProvider;

    public TemplateManagementService(
        ITemplateRegistry registry,
        ITemplateRepository repository,
        ICodeTemplateProvider codeTemplateProvider)
    {
        _registry = registry;
        _repository = repository;
        _codeTemplateProvider = codeTemplateProvider;
    }

    public async Task<IReadOnlyList<TemplateListItemDto>> ListAsync(
        TemplateKind kind,
        CancellationToken cancellationToken = default)
    {
        var templates = await _registry.GetAllAsync(kind, cancellationToken);
        var results = new List<TemplateListItemDto>(templates.Count);

        foreach (var template in templates)
        {
            var usage = await GetUsageCountAsync(template.Key, kind, cancellationToken);
            results.Add(new TemplateListItemDto(
                template.Key,
                template.DisplayName,
                template.Description,
                template.Kind,
                ResolveSourceKind(template),
                template.Properties.Count,
                usage));
        }

        return results.OrderBy(t => t.DisplayName).ToList();
    }

    public async Task<TemplateDetailDto> GetDetailAsync(string key, CancellationToken cancellationToken = default)
    {
        var resolved = await ResolveTemplateAsync(key, cancellationToken);
        return await MapDetailAsync(resolved, cancellationToken);
    }

    public async Task<TemplateDetailDto> CreateDynamicTemplateAsync(
        CreateDynamicTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedKey = NormalizeKey(request.Key);

        if (_codeTemplateProvider.GetTemplates().Any(t =>
                string.Equals(t.Key, normalizedKey, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Template key '{normalizedKey}' is already defined in code.");
        }

        if (await _repository.TemplateKeyExistsAsync(normalizedKey, cancellationToken))
        {
            throw new InvalidOperationException($"Template key '{normalizedKey}' already exists.");
        }

        await _repository.CreateTemplateAsync(
            new CreatePersistedTemplateRequest(
                normalizedKey,
                request.DisplayName.Trim(),
                request.Description,
                request.Kind,
                IsDynamic: true),
            cancellationToken);

        await _repository.SaveChangesAsync(cancellationToken);
        return await GetDetailAsync(normalizedKey, cancellationToken);
    }

    public async Task<TemplateDetailDto> UpdateDynamicTemplateAsync(
        string key,
        UpdateDynamicTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedKey = NormalizeKey(key);
        var persisted = await _repository.GetPersistedTemplateAsync(normalizedKey, cancellationToken)
            ?? throw new InvalidOperationException($"Template '{normalizedKey}' was not found in the database.");

        if (!persisted.IsDynamic)
        {
            throw new InvalidOperationException("Only dynamic templates can have their metadata edited.");
        }

        await _repository.UpdateTemplateMetadataAsync(
            new UpdatePersistedTemplateRequest(
                persisted.Id,
                request.DisplayName.Trim(),
                request.Description),
            cancellationToken);

        await _repository.SaveChangesAsync(cancellationToken);
        return await GetDetailAsync(normalizedKey, cancellationToken);
    }

    public async Task<TemplateDetailDto> AddPropertyAsync(
        string templateKey,
        CreateTemplatePropertyRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedKey = NormalizeKey(templateKey);
        var resolved = await ResolveTemplateAsync(normalizedKey, cancellationToken);
        var propertyName = NormalizePropertyName(request.Name);

        if (resolved.Properties.Any(p =>
                string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Property '{propertyName}' already exists on this template.");
        }

        var templateId = await EnsurePersistedTemplateForPropertyAsync(resolved, cancellationToken);

        await _repository.AddPropertyAsync(
            templateId,
            MapCreateProperty(request, propertyName),
            cancellationToken);

        await _repository.SaveChangesAsync(cancellationToken);
        return await GetDetailAsync(normalizedKey, cancellationToken);
    }

    public async Task<TemplateDetailDto> UpdatePropertyAsync(
        string templateKey,
        Guid propertyId,
        UpdateTemplatePropertyRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedKey = NormalizeKey(templateKey);
        var resolved = await ResolveTemplateAsync(normalizedKey, cancellationToken);
        var persistedProperty = await _repository.GetPropertyAsync(propertyId, cancellationToken)
            ?? throw new InvalidOperationException($"Property '{propertyId}' was not found.");

        var resolvedProperty = resolved.Properties.FirstOrDefault(p =>
            string.Equals(p.Name, persistedProperty.Name, StringComparison.OrdinalIgnoreCase));

        if (resolvedProperty is null || !CanModifyProperty(resolvedProperty))
        {
            throw new InvalidOperationException("Code-defined properties cannot be edited.");
        }

        await _repository.UpdatePropertyAsync(propertyId, MapUpdateProperty(request), cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return await GetDetailAsync(normalizedKey, cancellationToken);
    }

    public async Task<TemplateDetailDto> DeletePropertyAsync(
        string templateKey,
        Guid propertyId,
        CancellationToken cancellationToken = default)
    {
        var normalizedKey = NormalizeKey(templateKey);
        var resolved = await ResolveTemplateAsync(normalizedKey, cancellationToken);
        var persistedProperty = await _repository.GetPropertyAsync(propertyId, cancellationToken)
            ?? throw new InvalidOperationException($"Property '{propertyId}' was not found.");

        var resolvedProperty = resolved.Properties.FirstOrDefault(p =>
            string.Equals(p.Name, persistedProperty.Name, StringComparison.OrdinalIgnoreCase));

        if (resolvedProperty is null || !CanModifyProperty(resolvedProperty))
        {
            throw new InvalidOperationException("Code-defined properties cannot be deleted.");
        }

        if (await IsPropertyInUseAsync(normalizedKey, resolved.Kind, persistedProperty.Name, cancellationToken))
        {
            throw new InvalidOperationException(
                $"Property '{persistedProperty.Name}' is in use by existing {(resolved.Kind == TemplateKind.Content ? "content" : "products")} and cannot be deleted.");
        }

        await _repository.DeletePropertyAsync(propertyId, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return await GetDetailAsync(normalizedKey, cancellationToken);
    }

    public async Task<TemplateDetailDto> ReorderPropertiesAsync(
        string templateKey,
        IReadOnlyList<PropertySortOrderUpdate> updates,
        CancellationToken cancellationToken = default)
    {
        var normalizedKey = NormalizeKey(templateKey);
        await ResolveTemplateAsync(normalizedKey, cancellationToken);

        if (updates.Count > 0)
        {
            await _repository.UpdatePropertySortOrdersAsync(updates, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
        }

        return await GetDetailAsync(normalizedKey, cancellationToken);
    }

    private async Task<Guid> EnsurePersistedTemplateForPropertyAsync(
        ResolvedTemplateDefinition resolved,
        CancellationToken cancellationToken)
    {
        var persisted = await _repository.GetPersistedTemplateAsync(resolved.Key, cancellationToken);
        if (persisted is not null)
        {
            return persisted.Id;
        }

        if (resolved.IsDynamic)
        {
            throw new InvalidOperationException($"Dynamic template '{resolved.Key}' is missing persisted metadata.");
        }

        return await _repository.EnsureExtensionTemplateAsync(
            new EnsureExtensionTemplateRequest(
                resolved.Key,
                $"{resolved.DisplayName} Extensions",
                resolved.Kind,
                resolved.Key),
            cancellationToken);
    }

    private async Task<TemplateDetailDto> MapDetailAsync(
        ResolvedTemplateDefinition resolved,
        CancellationToken cancellationToken)
    {
        var persisted = await _repository.GetPersistedTemplateAsync(resolved.Key, cancellationToken);
        var usage = await GetUsageCountAsync(resolved.Key, resolved.Kind, cancellationToken);

        var persistedProperties = persisted is null
            ? new Dictionary<string, PersistedPropertyDto>(StringComparer.OrdinalIgnoreCase)
            : (await _repository.GetPropertiesForTemplateAsync(persisted.Id, cancellationToken))
                .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

        var properties = resolved.Properties
            .OrderBy(p => p.SortOrder)
            .Select(p => MapPropertyDetail(p, persistedProperties))
            .ToList();

        return new TemplateDetailDto(
            resolved.Key,
            resolved.DisplayName,
            resolved.Description,
            resolved.Kind,
            ResolveSourceKind(resolved),
            usage,
            usage > 0,
            resolved.IsDynamic,
            properties);
    }

    private static TemplatePropertyDetailDto MapPropertyDetail(
        PropertyDefinition property,
        IReadOnlyDictionary<string, PersistedPropertyDto> persistedProperties)
    {
        persistedProperties.TryGetValue(property.Name, out var persisted);
        var canModify = CanModifyProperty(property);

        return new TemplatePropertyDetailDto(
            persisted?.Id,
            property.Name,
            property.DisplayName,
            property.Description,
            property.Type,
            property.IsRequired,
            property.SortOrder,
            property.DefaultValue,
            property.Source,
            canModify,
            canModify,
            property.Validation.MinLength,
            property.Validation.MaxLength,
            property.Validation.MinValue,
            property.Validation.MaxValue,
            property.Validation.Pattern,
            property.Options.Select(o => new SelectOptionDto(o.Value, o.Label, o.SortOrder)).ToList());
    }

    private static bool CanModifyProperty(PropertyDefinition property) =>
        property.Source is PropertySource.Extension or PropertySource.Dynamic;

    private async Task<ResolvedTemplateDefinition> ResolveTemplateAsync(
        string key,
        CancellationToken cancellationToken)
    {
        var normalizedKey = NormalizeKey(key);
        return await _registry.GetByKeyAsync(normalizedKey, cancellationToken)
            ?? throw new InvalidOperationException($"Template '{normalizedKey}' was not found.");
    }

    private async Task<int> GetUsageCountAsync(
        string key,
        TemplateKind kind,
        CancellationToken cancellationToken) =>
        kind == TemplateKind.Content
            ? await _repository.GetContentUsageCountAsync(key, cancellationToken)
            : await _repository.GetProductUsageCountAsync(key, cancellationToken);

    private async Task<bool> IsPropertyInUseAsync(
        string templateKey,
        TemplateKind kind,
        string propertyName,
        CancellationToken cancellationToken) =>
        kind == TemplateKind.Content
            ? await _repository.IsContentPropertyInUseAsync(templateKey, propertyName, cancellationToken)
            : await _repository.IsProductPropertyInUseAsync(templateKey, propertyName, cancellationToken);

    private static TemplateSourceKind ResolveSourceKind(ResolvedTemplateDefinition template)
    {
        if (template.IsDynamic)
        {
            return TemplateSourceKind.Dynamic;
        }

        return template.Properties.Any(p => p.Source == PropertySource.Extension)
            ? TemplateSourceKind.Extended
            : TemplateSourceKind.Code;
    }

    private static string NormalizeKey(string key) =>
        key.Trim().ToLowerInvariant();

    private static string NormalizePropertyName(string name)
    {
        var normalized = name.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new InvalidOperationException("Property name is required.");
        }

        return normalized;
    }

    private static CreatePropertyPersistenceRequest MapCreateProperty(
        CreateTemplatePropertyRequest request,
        string propertyName) =>
        new(
            propertyName,
            request.DisplayName.Trim(),
            request.Description,
            request.Type,
            request.IsRequired,
            request.SortOrder,
            request.DefaultValue,
            request.MinLength,
            request.MaxLength,
            request.MinValue,
            request.MaxValue,
            request.Pattern,
            request.Options);

    private static UpdatePropertyPersistenceRequest MapUpdateProperty(UpdateTemplatePropertyRequest request) =>
        new(
            request.DisplayName.Trim(),
            request.Description,
            request.IsRequired,
            request.SortOrder,
            request.DefaultValue,
            request.MinLength,
            request.MaxLength,
            request.MinValue,
            request.MaxValue,
            request.Pattern,
            request.Options);
}