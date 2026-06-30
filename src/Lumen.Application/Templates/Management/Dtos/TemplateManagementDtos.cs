using Lumen.Domain.Enums;

namespace Lumen.Application.Templates.Management.Dtos;

public enum TemplateSourceKind
{
    Code,
    Extended,
    Dynamic
}

public sealed record TemplateListItemDto(
    string Key,
    string DisplayName,
    string? Description,
    TemplateKind Kind,
    TemplateSourceKind Source,
    int PropertyCount,
    int UsageCount);

public sealed record TemplateDetailDto(
    string Key,
    string DisplayName,
    string? Description,
    TemplateKind Kind,
    TemplateSourceKind Source,
    int UsageCount,
    bool IsInUse,
    bool CanEditMetadata,
    IReadOnlyList<TemplatePropertyDetailDto> Properties);

public sealed record TemplatePropertyDetailDto(
    Guid? PersistedId,
    string Name,
    string DisplayName,
    string? Description,
    PropertyType Type,
    bool IsRequired,
    int SortOrder,
    string? DefaultValue,
    PropertySource Source,
    bool CanEdit,
    bool CanDelete,
    int? MinLength,
    int? MaxLength,
    decimal? MinValue,
    decimal? MaxValue,
    string? Pattern,
    IReadOnlyList<SelectOptionDto> Options);

public sealed record SelectOptionDto(string Value, string Label, int SortOrder);

public sealed record CreateDynamicTemplateRequest(
    string Key,
    string DisplayName,
    string? Description,
    TemplateKind Kind);

public sealed record UpdateDynamicTemplateRequest(
    string DisplayName,
    string? Description);

public sealed record CreateTemplatePropertyRequest(
    string Name,
    string DisplayName,
    string? Description,
    PropertyType Type,
    bool IsRequired,
    int SortOrder,
    string? DefaultValue,
    int? MinLength,
    int? MaxLength,
    decimal? MinValue,
    decimal? MaxValue,
    string? Pattern,
    IReadOnlyList<SelectOptionDto>? Options);

public sealed record UpdateTemplatePropertyRequest(
    string DisplayName,
    string? Description,
    bool IsRequired,
    int SortOrder,
    string? DefaultValue,
    int? MinLength,
    int? MaxLength,
    decimal? MinValue,
    decimal? MaxValue,
    string? Pattern,
    IReadOnlyList<SelectOptionDto>? Options);

public sealed record PropertySortOrderUpdate(Guid PropertyId, int SortOrder);

// Persistence-layer DTOs (repository boundary)

public sealed record PersistedTemplateDto(
    Guid Id,
    string Key,
    string DisplayName,
    string? Description,
    TemplateKind Kind,
    string? BaseTemplateKey,
    bool IsDynamic);

public sealed record PersistedPropertyDto(
    Guid Id,
    Guid TemplateId,
    string Name,
    string DisplayName,
    string? Description,
    PropertyType Type,
    bool IsRequired,
    int SortOrder,
    string? DefaultValue,
    string? ReferenceTemplateKey,
    int? MinLength,
    int? MaxLength,
    decimal? MinValue,
    decimal? MaxValue,
    string? Pattern,
    IReadOnlyList<SelectOptionDto> Options);

public sealed record CreatePersistedTemplateRequest(
    string Key,
    string DisplayName,
    string? Description,
    TemplateKind Kind,
    bool IsDynamic);

public sealed record UpdatePersistedTemplateRequest(
    Guid TemplateId,
    string DisplayName,
    string? Description);

public sealed record EnsureExtensionTemplateRequest(
    string Key,
    string DisplayName,
    TemplateKind Kind,
    string BaseTemplateKey);

public sealed record CreatePropertyPersistenceRequest(
    string Name,
    string DisplayName,
    string? Description,
    PropertyType Type,
    bool IsRequired,
    int SortOrder,
    string? DefaultValue,
    int? MinLength,
    int? MaxLength,
    decimal? MinValue,
    decimal? MaxValue,
    string? Pattern,
    IReadOnlyList<SelectOptionDto>? Options);

public sealed record UpdatePropertyPersistenceRequest(
    string DisplayName,
    string? Description,
    bool IsRequired,
    int SortOrder,
    string? DefaultValue,
    int? MinLength,
    int? MaxLength,
    decimal? MinValue,
    decimal? MaxValue,
    string? Pattern,
    IReadOnlyList<SelectOptionDto>? Options);