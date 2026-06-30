using Lumen.Application.Products.Dtos;
using Lumen.Application.Templates;
using Lumen.Application.Templates.Validation;
using Lumen.Domain.Enums;
using Lumen.Domain.Products;
using Lumen.Domain.Templates;

namespace Lumen.Application.Products;

public sealed class ProductService : IProductService
{
    private readonly ITemplateRegistry _templateRegistry;
    private readonly IProductRepository _repository;
    private readonly TemplateValidator _validator;

    public ProductService(
        ITemplateRegistry templateRegistry,
        IProductRepository repository,
        TemplateValidator validator)
    {
        _templateRegistry = templateRegistry;
        _repository = repository;
        _validator = validator;
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _repository.GetByIdAsync(id, cancellationToken);
        return product is null ? null : await MapWithVariantsAsync(product, cancellationToken);
    }

    public async Task<ProductDto?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        var product = await _repository.GetBySkuAsync(sku, cancellationToken);
        return product is null ? null : await MapWithVariantsAsync(product, cancellationToken);
    }

    public async Task<IReadOnlyList<ProductDto>> ListAsync(
        Guid? categoryId,
        ProductStatus? status,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var products = await _repository.ListAsync(categoryId, status, search, cancellationToken);
        var results = new List<ProductDto>(products.Count);

        foreach (var product in products)
        {
            results.Add(await MapWithVariantsAsync(product, cancellationToken));
        }

        return results;
    }

    public async Task<ProductDto> CreateAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var template = await ResolveProductTemplateAsync(request.TemplateKey, cancellationToken);
        var hydrated = _validator.Hydrate(template, request.Properties);
        var sku = ResolveSku(request.Sku, hydrated);
        var name = ResolveName(request.Name, hydrated);

        await EnsureSkuAvailableAsync(sku, cancellationToken: cancellationToken);

        var product = Product.Create(
            template.Key,
            sku,
            name,
            request.CategoryId,
            request.SortOrder,
            hydrated);

        await _repository.AddAsync(product, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return await MapWithVariantsAsync(product, cancellationToken);
    }

    public async Task<ProductDto> UpdateAsync(
        Guid id,
        UpdateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException($"Product '{id}' was not found.");

        var template = await ResolveProductTemplateAsync(product.TemplateKey, cancellationToken);
        var hydrated = _validator.Hydrate(template, request.Properties);
        var sku = ResolveSku(request.Sku, hydrated);
        var name = ResolveName(request.Name, hydrated);

        await EnsureSkuAvailableAsync(sku, excludeProductId: id, cancellationToken: cancellationToken);

        product.Update(sku, name, request.CategoryId, request.SortOrder);
        product.SetProperties(hydrated);

        await _repository.UpdateAsync(product, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return await MapWithVariantsAsync(product, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException($"Product '{id}' was not found.");

        await _repository.DeleteAsync(product, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<ProductDto> PublishAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException($"Product '{id}' was not found.");

        product.Publish();
        await _repository.UpdateAsync(product, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return await MapWithVariantsAsync(product, cancellationToken);
    }

    public async Task<ProductDto> UnpublishAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException($"Product '{id}' was not found.");

        product.Unpublish();
        await _repository.UpdateAsync(product, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return await MapWithVariantsAsync(product, cancellationToken);
    }

    public async Task<IReadOnlyList<ProductVariantDto>> GetVariantsAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        await EnsureProductExistsAsync(productId, cancellationToken);
        var variants = await _repository.GetVariantsAsync(productId, cancellationToken);
        return variants.Select(MapVariant).ToList();
    }

    public async Task<ProductVariantDto> CreateVariantAsync(
        Guid productId,
        CreateVariantRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = await _repository.GetByIdAsync(productId, cancellationToken)
            ?? throw new InvalidOperationException($"Product '{productId}' was not found.");

        await EnsureSkuAvailableAsync(request.Sku, cancellationToken: cancellationToken);

        var variant = ProductVariant.Create(
            product.Id,
            request.Sku,
            request.Name,
            request.SortOrder,
            request.Properties);

        await _repository.AddVariantAsync(variant, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return MapVariant(variant);
    }

    public async Task<ProductVariantDto> UpdateVariantAsync(
        Guid productId,
        Guid variantId,
        UpdateVariantRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureProductExistsAsync(productId, cancellationToken);

        var variant = await _repository.GetVariantByIdAsync(variantId, cancellationToken)
            ?? throw new InvalidOperationException($"Variant '{variantId}' was not found.");

        if (variant.ProductId != productId)
        {
            throw new InvalidOperationException($"Variant '{variantId}' does not belong to product '{productId}'.");
        }

        await EnsureSkuAvailableAsync(request.Sku, excludeVariantId: variantId, cancellationToken: cancellationToken);

        variant.Update(request.Sku, request.Name, request.SortOrder, request.Properties);
        await _repository.UpdateVariantAsync(variant, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return MapVariant(variant);
    }

    public async Task DeleteVariantAsync(
        Guid productId,
        Guid variantId,
        CancellationToken cancellationToken = default)
    {
        await EnsureProductExistsAsync(productId, cancellationToken);

        var variant = await _repository.GetVariantByIdAsync(variantId, cancellationToken)
            ?? throw new InvalidOperationException($"Variant '{variantId}' was not found.");

        if (variant.ProductId != productId)
        {
            throw new InvalidOperationException($"Variant '{variantId}' does not belong to product '{productId}'.");
        }

        await _repository.DeleteVariantAsync(variant, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureProductExistsAsync(Guid productId, CancellationToken cancellationToken)
    {
        if (await _repository.GetByIdAsync(productId, cancellationToken) is null)
        {
            throw new InvalidOperationException($"Product '{productId}' was not found.");
        }
    }

    private async Task EnsureSkuAvailableAsync(
        string sku,
        Guid? excludeProductId = null,
        Guid? excludeVariantId = null,
        CancellationToken cancellationToken = default)
    {
        if (await _repository.SkuExistsAsync(sku, excludeProductId, excludeVariantId, cancellationToken))
        {
            throw new InvalidOperationException($"SKU '{sku}' is already in use.");
        }
    }

    private async Task<ResolvedTemplateDefinition> ResolveProductTemplateAsync(
        string templateKey,
        CancellationToken cancellationToken)
    {
        var template = await _templateRegistry.GetByKeyAsync(templateKey, cancellationToken)
            ?? throw new InvalidOperationException($"Product template '{templateKey}' was not found.");

        if (template.Kind != TemplateKind.Product)
        {
            throw new InvalidOperationException($"Template '{templateKey}' is not a product template.");
        }

        return template;
    }

    private static string ResolveSku(string requestSku, IDictionary<string, object?> hydrated)
    {
        if (hydrated.TryGetValue("sku", out var hydratedSku) && hydratedSku is not null)
        {
            return hydratedSku.ToString() ?? requestSku;
        }

        return requestSku;
    }

    private static string ResolveName(string requestName, IDictionary<string, object?> hydrated)
    {
        if (hydrated.TryGetValue("name", out var hydratedName) && hydratedName is not null)
        {
            return hydratedName.ToString() ?? requestName;
        }

        return requestName;
    }

    private async Task<ProductDto> MapWithVariantsAsync(Product product, CancellationToken cancellationToken)
    {
        var variants = await _repository.GetVariantsAsync(product.Id, cancellationToken);
        return Map(product, variants);
    }

    private static ProductDto Map(Product product, IReadOnlyList<ProductVariant> variants) =>
        new(
            product.Id,
            product.TemplateKey,
            product.Sku,
            product.Name,
            product.CategoryId,
            product.SortOrder,
            product.Status,
            product.PublishedAt,
            product.CreatedAt,
            product.UpdatedAt,
            product.Properties,
            variants.Select(MapVariant).ToList());

    private static ProductVariantDto MapVariant(ProductVariant variant) =>
        new(
            variant.Id,
            variant.ProductId,
            variant.Sku,
            variant.Name,
            variant.SortOrder,
            variant.Status,
            variant.CreatedAt,
            variant.UpdatedAt,
            variant.Properties);
}