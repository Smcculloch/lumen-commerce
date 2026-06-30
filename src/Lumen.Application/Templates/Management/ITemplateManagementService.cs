using Lumen.Application.Templates.Management.Dtos;
using Lumen.Domain.Enums;

namespace Lumen.Application.Templates.Management;

public interface ITemplateManagementService
{
    Task<IReadOnlyList<TemplateListItemDto>> ListAsync(TemplateKind kind, CancellationToken cancellationToken = default);
    Task<TemplateDetailDto> GetDetailAsync(string key, CancellationToken cancellationToken = default);
    Task<TemplateDetailDto> CreateDynamicTemplateAsync(CreateDynamicTemplateRequest request, CancellationToken cancellationToken = default);
    Task<TemplateDetailDto> UpdateDynamicTemplateAsync(string key, UpdateDynamicTemplateRequest request, CancellationToken cancellationToken = default);
    Task<TemplateDetailDto> AddPropertyAsync(string templateKey, CreateTemplatePropertyRequest request, CancellationToken cancellationToken = default);
    Task<TemplateDetailDto> UpdatePropertyAsync(string templateKey, Guid propertyId, UpdateTemplatePropertyRequest request, CancellationToken cancellationToken = default);
    Task<TemplateDetailDto> DeletePropertyAsync(string templateKey, Guid propertyId, CancellationToken cancellationToken = default);
    Task<TemplateDetailDto> ReorderPropertiesAsync(string templateKey, IReadOnlyList<PropertySortOrderUpdate> updates, CancellationToken cancellationToken = default);
}