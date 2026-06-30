using System.Text.Json;
using Lumen.Application.Templates.Management;
using Lumen.Application.Templates.Management.Dtos;
using Lumen.Infrastructure.Persistence;
using Lumen.Infrastructure.Persistence.Entities;
using Lumen.Infrastructure.Persistence.Mapping;
using Microsoft.EntityFrameworkCore;

namespace Lumen.Infrastructure.Repositories;

public sealed class TemplateRepository : ITemplateRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly AppDbContext _dbContext;

    public TemplateRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PersistedTemplateDto?> GetPersistedTemplateAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        var normalized = key.Trim().ToLowerInvariant();
        var entity = await _dbContext.TemplateDefinitions
            .FirstOrDefaultAsync(t => t.Key == normalized, cancellationToken);

        return entity is null ? null : TemplateMapping.ToDto(entity);
    }

    public Task<int> GetContentUsageCountAsync(string templateKey, CancellationToken cancellationToken = default)
    {
        var normalized = templateKey.Trim().ToLowerInvariant();
        return _dbContext.ContentItems.CountAsync(c => c.TemplateKey == normalized, cancellationToken);
    }

    public Task<int> GetProductUsageCountAsync(string templateKey, CancellationToken cancellationToken = default)
    {
        var normalized = templateKey.Trim().ToLowerInvariant();
        return _dbContext.Products.CountAsync(p => p.TemplateKey == normalized, cancellationToken);
    }

    public async Task<bool> IsContentPropertyInUseAsync(
        string templateKey,
        string propertyName,
        CancellationToken cancellationToken = default)
    {
        var normalized = templateKey.Trim().ToLowerInvariant();
        var jsonRows = await _dbContext.ContentItems
            .Where(c => c.TemplateKey == normalized)
            .Select(c => c.PropertiesJson)
            .ToListAsync(cancellationToken);

        return jsonRows.Any(json => PropertyHasValue(json, propertyName));
    }

    public async Task<bool> IsProductPropertyInUseAsync(
        string templateKey,
        string propertyName,
        CancellationToken cancellationToken = default)
    {
        var normalized = templateKey.Trim().ToLowerInvariant();
        var jsonRows = await _dbContext.Products
            .Where(p => p.TemplateKey == normalized)
            .Select(p => p.PropertiesJson)
            .ToListAsync(cancellationToken);

        return jsonRows.Any(json => PropertyHasValue(json, propertyName));
    }

    public Task<bool> TemplateKeyExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        var normalized = key.Trim().ToLowerInvariant();
        return _dbContext.TemplateDefinitions.AnyAsync(t => t.Key == normalized, cancellationToken);
    }

    public async Task<Guid> CreateTemplateAsync(
        CreatePersistedTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = new TemplateDefinitionEntity
        {
            Key = request.Key.Trim().ToLowerInvariant(),
            DisplayName = request.DisplayName,
            Description = request.Description,
            Kind = request.Kind,
            IsDynamic = request.IsDynamic,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _dbContext.TemplateDefinitions.AddAsync(entity, cancellationToken);
        return entity.Id;
    }

    public async Task UpdateTemplateMetadataAsync(
        UpdatePersistedTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.TemplateDefinitions.FindAsync([request.TemplateId], cancellationToken)
            ?? throw new InvalidOperationException($"Template '{request.TemplateId}' was not found.");

        entity.DisplayName = request.DisplayName;
        entity.Description = request.Description;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
    }

    public async Task<Guid> EnsureExtensionTemplateAsync(
        EnsureExtensionTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalized = request.Key.Trim().ToLowerInvariant();
        var existing = await _dbContext.TemplateDefinitions
            .FirstOrDefaultAsync(t => t.Key == normalized, cancellationToken);

        if (existing is not null)
        {
            return existing.Id;
        }

        var entity = new TemplateDefinitionEntity
        {
            Key = normalized,
            DisplayName = request.DisplayName,
            Description = $"Backoffice-managed extensions for the {request.Key} template.",
            Kind = request.Kind,
            BaseTemplateKey = request.BaseTemplateKey,
            IsDynamic = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _dbContext.TemplateDefinitions.AddAsync(entity, cancellationToken);
        return entity.Id;
    }

    public async Task<IReadOnlyList<PersistedPropertyDto>> GetPropertiesForTemplateAsync(
        Guid templateId,
        CancellationToken cancellationToken = default)
    {
        var entities = await _dbContext.PropertyDefinitions
            .Where(p => p.TemplateDefinitionId == templateId)
            .OrderBy(p => p.SortOrder)
            .ToListAsync(cancellationToken);

        return entities.Select(TemplateMapping.ToDto).ToList();
    }

    public async Task<PersistedPropertyDto?> GetPropertyAsync(
        Guid propertyId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.PropertyDefinitions
            .FirstOrDefaultAsync(p => p.Id == propertyId, cancellationToken);

        return entity is null ? null : TemplateMapping.ToDto(entity);
    }

    public async Task<Guid> AddPropertyAsync(
        Guid templateId,
        CreatePropertyPersistenceRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = TemplateMapping.ToEntity(templateId, request);
        await _dbContext.PropertyDefinitions.AddAsync(entity, cancellationToken);

        var template = await _dbContext.TemplateDefinitions.FindAsync([templateId], cancellationToken);
        if (template is not null)
        {
            template.UpdatedAt = DateTimeOffset.UtcNow;
        }

        return entity.Id;
    }

    public async Task UpdatePropertyAsync(
        Guid propertyId,
        UpdatePropertyPersistenceRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.PropertyDefinitions
            .Include(p => p.TemplateDefinition)
            .FirstOrDefaultAsync(p => p.Id == propertyId, cancellationToken)
            ?? throw new InvalidOperationException($"Property '{propertyId}' was not found.");

        TemplateMapping.ApplyUpdate(entity, request);
        entity.TemplateDefinition.UpdatedAt = DateTimeOffset.UtcNow;
    }

    public async Task DeletePropertyAsync(Guid propertyId, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.PropertyDefinitions
            .Include(p => p.TemplateDefinition)
            .FirstOrDefaultAsync(p => p.Id == propertyId, cancellationToken);

        if (entity is not null)
        {
            entity.TemplateDefinition.UpdatedAt = DateTimeOffset.UtcNow;
            _dbContext.PropertyDefinitions.Remove(entity);
        }
    }

    public async Task UpdatePropertySortOrdersAsync(
        IReadOnlyList<PropertySortOrderUpdate> updates,
        CancellationToken cancellationToken = default)
    {
        foreach (var update in updates)
        {
            var entity = await _dbContext.PropertyDefinitions.FindAsync([update.PropertyId], cancellationToken);
            if (entity is not null)
            {
                entity.SortOrder = update.SortOrder;
            }
        }
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);

    private static bool PropertyHasValue(string json, string propertyName)
    {
        var properties = JsonSerializer.Deserialize<Dictionary<string, object?>>(json, JsonOptions);
        if (properties is null)
        {
            return false;
        }

        foreach (var (key, value) in properties)
        {
            if (!string.Equals(key, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (value is null)
            {
                return false;
            }

            if (value is JsonElement element)
            {
                return element.ValueKind switch
                {
                    JsonValueKind.Null or JsonValueKind.Undefined => false,
                    JsonValueKind.String => !string.IsNullOrWhiteSpace(element.GetString()),
                    JsonValueKind.False => false,
                    _ => true
                };
            }

            return !string.IsNullOrWhiteSpace(value.ToString());
        }

        return false;
    }
}