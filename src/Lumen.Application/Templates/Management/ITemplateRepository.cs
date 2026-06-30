using Lumen.Application.Templates.Management.Dtos;
using Lumen.Domain.Enums;

namespace Lumen.Application.Templates.Management;

public interface ITemplateRepository
{
    Task<PersistedTemplateDto?> GetPersistedTemplateAsync(string key, CancellationToken cancellationToken = default);
    Task<int> GetContentUsageCountAsync(string templateKey, CancellationToken cancellationToken = default);
    Task<int> GetProductUsageCountAsync(string templateKey, CancellationToken cancellationToken = default);
    Task<bool> IsContentPropertyInUseAsync(string templateKey, string propertyName, CancellationToken cancellationToken = default);
    Task<bool> IsProductPropertyInUseAsync(string templateKey, string propertyName, CancellationToken cancellationToken = default);
    Task<bool> TemplateKeyExistsAsync(string key, CancellationToken cancellationToken = default);
    Task<Guid> CreateTemplateAsync(CreatePersistedTemplateRequest request, CancellationToken cancellationToken = default);
    Task UpdateTemplateMetadataAsync(UpdatePersistedTemplateRequest request, CancellationToken cancellationToken = default);
    Task<Guid> EnsureExtensionTemplateAsync(EnsureExtensionTemplateRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PersistedPropertyDto>> GetPropertiesForTemplateAsync(Guid templateId, CancellationToken cancellationToken = default);
    Task<PersistedPropertyDto?> GetPropertyAsync(Guid propertyId, CancellationToken cancellationToken = default);
    Task<Guid> AddPropertyAsync(Guid templateId, CreatePropertyPersistenceRequest request, CancellationToken cancellationToken = default);
    Task UpdatePropertyAsync(Guid propertyId, UpdatePropertyPersistenceRequest request, CancellationToken cancellationToken = default);
    Task DeletePropertyAsync(Guid propertyId, CancellationToken cancellationToken = default);
    Task UpdatePropertySortOrdersAsync(IReadOnlyList<PropertySortOrderUpdate> updates, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}