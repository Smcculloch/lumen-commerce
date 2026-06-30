using Lumen.Application.Content.Dtos;
using Lumen.Application.Templates;
using Lumen.Application.Templates.Validation;
using Lumen.Domain.Content;
using Lumen.Domain.Enums;


namespace Lumen.Application.Content;

public sealed class ContentService : IContentService
{
    private readonly ITemplateRegistry _templateRegistry;
    private readonly IContentRepository _repository;
    private readonly TemplateValidator _validator;

    public ContentService(
        ITemplateRegistry templateRegistry,
        IContentRepository repository,
        TemplateValidator validator)
    {
        _templateRegistry = templateRegistry;
        _repository = repository;
        _validator = validator;
    }

    public async Task<ContentDto?> GetContentByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken);
        return item is null ? null : Map(item);
    }

    public async Task<ContentDto?> GetContentBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var fullPath = slug.StartsWith('/') ? slug : $"/{slug.Trim().ToLowerInvariant()}";
        var item = await _repository.GetByFullPathAsync(fullPath, cancellationToken);
        return item is null ? null : Map(item);
    }

    public async Task<IReadOnlyList<ContentDto>> GetRootsAsync(CancellationToken cancellationToken = default)
    {
        var roots = await _repository.GetRootsAsync(cancellationToken);
        return roots.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<NavigationItemDto>> GetNavigationAsync(CancellationToken cancellationToken = default)
    {
        var roots = await _repository.GetRootsAsync(cancellationToken);
        return await ContentNavigationBuilder.BuildAsync(
            roots,
            _repository.GetChildrenAsync,
            cancellationToken);
    }

    public async Task<IReadOnlyList<ContentDto>> GetChildrenAsync(Guid parentId, CancellationToken cancellationToken = default)
    {
        var children = await _repository.GetChildrenAsync(parentId, cancellationToken);
        return children.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<ContentDto>> GetDescendantsAsync(Guid contentId, CancellationToken cancellationToken = default)
    {
        var descendants = await _repository.GetDescendantsAsync(contentId, cancellationToken);
        return descendants.Select(Map).ToList();
    }

    public async Task<ContentDto> CreateContentAsync(
        CreateContentRequest request,
        CancellationToken cancellationToken = default)
    {
        var template = await ResolveContentTemplateAsync(request.TemplateKey, cancellationToken);
        var parent = await ResolveParentAsync(request.ParentId, cancellationToken);

        var hydrated = _validator.Hydrate(template, request.Properties);
        var slug = await ResolveSlugAsync(request.Slug, request.Name, request.ParentId, cancellationToken);

        var item = ContentItem.Create(
            template.Key,
            request.Name,
            slug,
            request.ParentId,
            request.SortOrder,
            ContentPathHelper.BuildLevel(parent),
            materializedPath: "/",
            fullPath: ContentPathHelper.BuildFullPath(slug, parent),
            hydrated);

        item = ContentItem.Rehydrate(
            item.Id,
            item.TemplateKey,
            item.Name,
            item.Slug,
            item.ParentId,
            item.SortOrder,
            item.Level,
            ContentPathHelper.BuildMaterializedPath(item.Id, parent),
            item.FullPath,
            item.Status,
            item.PublishedAt,
            item.CreatedAt,
            item.UpdatedAt,
            item.Properties);

        await _repository.AddAsync(item, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return Map(item);
    }

    public async Task<ContentDto> UpdateContentAsync(
        Guid id,
        UpdateContentRequest request,
        CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException($"Content '{id}' was not found.");

        var template = await ResolveContentTemplateAsync(item.TemplateKey, cancellationToken);
        var hydrated = _validator.Hydrate(template, request.Properties);

        var parent = request.ParentId == item.ParentId
            ? await ResolveParentAsync(item.ParentId, cancellationToken)
            : await ResolveParentAsync(request.ParentId, cancellationToken);

        if (request.ParentId == item.Id || IsDescendantParent(item, request.ParentId))
        {
            throw new InvalidOperationException("A content item cannot be moved under itself or its descendants.");
        }

        var slug = await ResolveSlugAsync(
            request.Slug ?? item.Slug,
            request.Name,
            request.ParentId,
            cancellationToken,
            excludeId: item.Id);

        item.UpdateMetadata(request.Name, slug, request.SortOrder ?? item.SortOrder);
        item.SetProperties(hydrated);

        var materializedPath = ContentPathHelper.BuildMaterializedPath(item.Id, parent);
        var fullPath = ContentPathHelper.BuildFullPath(slug, parent);
        item.SetHierarchy(request.ParentId, ContentPathHelper.BuildLevel(parent), materializedPath, fullPath);

        await _repository.UpdateAsync(item, cancellationToken);

        var descendants = await _repository.GetDescendantsAsync(item.Id, cancellationToken);
        foreach (var descendant in descendants)
        {
            await RecomputeDescendantPathsAsync(descendant, cancellationToken);
            await _repository.UpdateAsync(descendant, cancellationToken);
        }

        await _repository.SaveChangesAsync(cancellationToken);
        return Map(item);
    }

    public async Task<ContentDto> PublishContentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException($"Content '{id}' was not found.");

        var template = await ResolveContentTemplateAsync(item.TemplateKey, cancellationToken);
        _validator.Validate(template, item.Properties);

        item.Publish();
        await _repository.UpdateAsync(item, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return Map(item);
    }

    public async Task<ContentDto> UnpublishContentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException($"Content '{id}' was not found.");

        item.Unpublish();
        await _repository.UpdateAsync(item, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return Map(item);
    }

    public async Task DeleteContentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException($"Content '{id}' was not found.");

        if (await _repository.HasChildrenAsync(id, cancellationToken))
        {
            throw new InvalidOperationException("Cannot delete content that still has child nodes.");
        }

        await _repository.DeleteAsync(item, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    private async Task RecomputeDescendantPathsAsync(ContentItem descendant, CancellationToken cancellationToken)
    {
        var parent = descendant.ParentId is Guid parentId
            ? await _repository.GetByIdAsync(parentId, cancellationToken)
            : null;

        if (parent is null)
        {
            throw new InvalidOperationException($"Parent '{descendant.ParentId}' was not found while recomputing paths.");
        }

        descendant.SetHierarchy(
            descendant.ParentId,
            ContentPathHelper.BuildLevel(parent),
            ContentPathHelper.BuildMaterializedPath(descendant.Id, parent),
            ContentPathHelper.BuildFullPath(descendant.Slug!, parent));
    }

    private static bool IsDescendantParent(ContentItem item, Guid? newParentId) =>
        newParentId is Guid parentId && parentId != item.ParentId &&
        item.MaterializedPath.Contains($"/{parentId}/", StringComparison.Ordinal);

    private async Task<string> ResolveSlugAsync(
        string? requestedSlug,
        string name,
        Guid? parentId,
        CancellationToken cancellationToken,
        Guid? excludeId = null)
    {
        var baseSlug = string.IsNullOrWhiteSpace(requestedSlug)
            ? ContentPathHelper.GenerateSlug(name)
            : requestedSlug.Trim().ToLowerInvariant();

        var siblings = await _repository.GetSiblingSlugsAsync(parentId, cancellationToken);
        if (excludeId is Guid excluded)
        {
            var current = await _repository.GetByIdAsync(excluded, cancellationToken);
            if (current?.Slug is not null)
            {
                siblings = siblings.Where(s => !string.Equals(s, current.Slug, StringComparison.OrdinalIgnoreCase)).ToList();
            }
        }

        return ContentPathHelper.EnsureUniqueSlug(baseSlug, siblings);
    }

    private async Task<ContentItem?> ResolveParentAsync(Guid? parentId, CancellationToken cancellationToken)
    {
        if (parentId is null)
        {
            return null;
        }

        return await _repository.GetByIdAsync(parentId.Value, cancellationToken)
            ?? throw new InvalidOperationException($"Parent content '{parentId}' was not found.");
    }

    private async Task<Domain.Templates.ResolvedTemplateDefinition> ResolveContentTemplateAsync(
        string templateKey,
        CancellationToken cancellationToken)
    {
        var template = await _templateRegistry.GetByKeyAsync(templateKey, cancellationToken)
            ?? throw new InvalidOperationException($"Content template '{templateKey}' was not found.");

        if (template.Kind != TemplateKind.Content)
        {
            throw new InvalidOperationException($"Template '{templateKey}' is not a content template.");
        }

        return template;
    }

    private static ContentDto Map(ContentItem item) =>
        new(
            item.Id,
            item.TemplateKey,
            item.Name,
            item.Slug!,
            item.FullPath,
            item.ParentId,
            item.SortOrder,
            item.Level,
            item.Status,
            item.PublishedAt,
            item.CreatedAt,
            item.UpdatedAt,
            item.Properties);
}