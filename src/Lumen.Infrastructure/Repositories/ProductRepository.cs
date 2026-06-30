using Lumen.Application.Products;
using Lumen.Domain.Enums;
using Lumen.Domain.Products;
using Lumen.Infrastructure.Persistence;
using Lumen.Infrastructure.Persistence.Mapping;
using Microsoft.EntityFrameworkCore;

namespace Lumen.Infrastructure.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly AppDbContext _dbContext;

    public ProductRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Products.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return entity is null ? null : InstanceMapping.ToDomain(entity);
    }

    public async Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        var normalized = sku.Trim();
        var entity = await _dbContext.Products.FirstOrDefaultAsync(x => x.Sku == normalized, cancellationToken);
        return entity is null ? null : InstanceMapping.ToDomain(entity);
    }

    public async Task<IReadOnlyList<Product>> ListAsync(
        Guid? categoryId,
        ProductStatus? status,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Products.AsQueryable();

        if (categoryId is Guid category)
        {
            query = query.Where(x => x.CategoryId == category);
        }

        if (status is ProductStatus productStatus)
        {
            query = query.Where(x => x.Status == productStatus);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                x.Name.Contains(search) ||
                x.Sku.Contains(search) ||
                x.TemplateKey.Contains(search));
        }

        var entities = await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return entities.Select(InstanceMapping.ToDomain).ToList();
    }

    public async Task<bool> SkuExistsAsync(
        string sku,
        Guid? excludeProductId = null,
        Guid? excludeVariantId = null,
        CancellationToken cancellationToken = default)
    {
        var normalized = sku.Trim();

        var productExists = await _dbContext.Products.AnyAsync(
            x => x.Sku == normalized && (excludeProductId == null || x.Id != excludeProductId),
            cancellationToken);

        if (productExists)
        {
            return true;
        }

        return await _dbContext.ProductVariants.AnyAsync(
            x => x.Sku == normalized && (excludeVariantId == null || x.Id != excludeVariantId),
            cancellationToken);
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _dbContext.Products.AddAsync(InstanceMapping.ToEntity(product), cancellationToken);
    }

    public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Products.FindAsync([product.Id], cancellationToken)
            ?? throw new InvalidOperationException($"Product '{product.Id}' was not found.");

        _dbContext.Entry(entity).CurrentValues.SetValues(InstanceMapping.ToEntity(product));
    }

    public async Task DeleteAsync(Product product, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Products.FindAsync([product.Id], cancellationToken);
        if (entity is not null)
        {
            _dbContext.Products.Remove(entity);
        }
    }

    public Task<bool> HasProductsInCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default) =>
        _dbContext.Products.AnyAsync(x => x.CategoryId == categoryId, cancellationToken);

    public async Task<IReadOnlyList<ProductVariant>> GetVariantsAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var entities = await _dbContext.ProductVariants
            .Where(x => x.ProductId == productId)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return entities.Select(InstanceMapping.ToDomain).ToList();
    }

    public async Task<ProductVariant?> GetVariantByIdAsync(Guid variantId, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.ProductVariants.FirstOrDefaultAsync(x => x.Id == variantId, cancellationToken);
        return entity is null ? null : InstanceMapping.ToDomain(entity);
    }

    public async Task AddVariantAsync(ProductVariant variant, CancellationToken cancellationToken = default)
    {
        await _dbContext.ProductVariants.AddAsync(InstanceMapping.ToEntity(variant), cancellationToken);
    }

    public async Task UpdateVariantAsync(ProductVariant variant, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.ProductVariants.FindAsync([variant.Id], cancellationToken)
            ?? throw new InvalidOperationException($"Variant '{variant.Id}' was not found.");

        _dbContext.Entry(entity).CurrentValues.SetValues(InstanceMapping.ToEntity(variant));
    }

    public async Task DeleteVariantAsync(ProductVariant variant, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.ProductVariants.FindAsync([variant.Id], cancellationToken);
        if (entity is not null)
        {
            _dbContext.ProductVariants.Remove(entity);
        }
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}