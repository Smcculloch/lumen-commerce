using Lumen.Domain.Categories;
using Lumen.Domain.Enums;
using Lumen.Domain.Products;
using Lumen.Infrastructure.Persistence.Entities;
using Lumen.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace Lumen.Infrastructure.Persistence.Seed;

/// <summary>
/// Seeds demo categories and products for the PIM module.
/// </summary>
public static class PimSeedData
{
    private static readonly Guid ElectronicsId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid LaptopsId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly Guid SmartphonesId = Guid.Parse("55555555-5555-5555-5555-555555555555");
    private static readonly Guid LaptopProductId = Guid.Parse("66666666-6666-6666-6666-666666666666");
    private static readonly Guid PhoneProductId = Guid.Parse("77777777-7777-7777-7777-777777777777");
    private static readonly Guid HeadphonesProductId = Guid.Parse("88888888-8888-8888-8888-888888888888");
    private static readonly Guid PhoneVariantId = Guid.Parse("99999999-9999-9999-9999-999999999999");

    public static async Task SeedAsync(AppDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await SeedCategoriesAsync(dbContext, cancellationToken);
        await SeedProductsAsync(dbContext, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedCategoriesAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
        if (!await dbContext.Categories.AnyAsync(c => c.Id == ElectronicsId, cancellationToken))
        {
            dbContext.Categories.Add(ToEntity(BuildElectronics()));
        }

        if (!await dbContext.Categories.AnyAsync(c => c.Id == LaptopsId, cancellationToken))
        {
            dbContext.Categories.Add(ToEntity(BuildLaptops()));
        }

        if (!await dbContext.Categories.AnyAsync(c => c.Id == SmartphonesId, cancellationToken))
        {
            dbContext.Categories.Add(ToEntity(BuildSmartphones()));
        }
    }

    private static async Task SeedProductsAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
        if (!await dbContext.Products.AnyAsync(p => p.Id == LaptopProductId, cancellationToken))
        {
            dbContext.Products.Add(ToEntity(BuildLaptopProduct()));
        }

        if (!await dbContext.Products.AnyAsync(p => p.Id == PhoneProductId, cancellationToken))
        {
            dbContext.Products.Add(ToEntity(BuildPhoneProduct()));
        }

        if (!await dbContext.Products.AnyAsync(p => p.Id == HeadphonesProductId, cancellationToken))
        {
            dbContext.Products.Add(ToEntity(BuildHeadphonesProduct()));
        }

        if (!await dbContext.ProductVariants.AnyAsync(v => v.Id == PhoneVariantId, cancellationToken))
        {
            dbContext.ProductVariants.Add(ToEntity(BuildPhoneVariant()));
        }
    }

    private static Category BuildElectronics()
    {
        var slug = "electronics";
        var category = Category.Create(
            "Electronics",
            slug,
            parentId: null,
            sortOrder: 0,
            level: 0,
            materializedPath: $"/{ElectronicsId}/",
            fullPath: $"/{slug}");

        return Category.Rehydrate(
            ElectronicsId,
            category.Name,
            category.Slug,
            category.ParentId,
            category.SortOrder,
            category.Level,
            category.MaterializedPath,
            category.FullPath,
            category.CreatedAt,
            category.UpdatedAt);
    }

    private static Category BuildLaptops()
    {
        var slug = "laptops";
        var category = Category.Create(
            "Laptops",
            slug,
            parentId: ElectronicsId,
            sortOrder: 0,
            level: 1,
            materializedPath: $"/{ElectronicsId}/{LaptopsId}/",
            fullPath: "/electronics/laptops");

        return Category.Rehydrate(
            LaptopsId,
            category.Name,
            category.Slug,
            category.ParentId,
            category.SortOrder,
            category.Level,
            category.MaterializedPath,
            category.FullPath,
            category.CreatedAt,
            category.UpdatedAt);
    }

    private static Category BuildSmartphones()
    {
        var slug = "smartphones";
        var category = Category.Create(
            "Smartphones",
            slug,
            parentId: ElectronicsId,
            sortOrder: 10,
            level: 1,
            materializedPath: $"/{ElectronicsId}/{SmartphonesId}/",
            fullPath: "/electronics/smartphones");

        return Category.Rehydrate(
            SmartphonesId,
            category.Name,
            category.Slug,
            category.ParentId,
            category.SortOrder,
            category.Level,
            category.MaterializedPath,
            category.FullPath,
            category.CreatedAt,
            category.UpdatedAt);
    }

    private static Product BuildLaptopProduct()
    {
        var properties = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["name"] = "Lumen Pro Laptop 15",
            ["sku"] = "LUM-LAP-15",
            ["shortDescription"] = "Lightweight developer laptop with all-day battery.",
            ["description"] = "<p>15-inch display, 16GB RAM, 512GB SSD. Seeded as a published laptop product.</p>",
            ["price"] = 1299.00m,
            ["compareAtPrice"] = 1499.00m,
            ["isActive"] = true,
            ["category"] = "electronics"
        };

        var product = Product.Create(
            TemplateKeys.StandardProduct,
            "LUM-LAP-15",
            "Lumen Pro Laptop 15",
            LaptopsId,
            sortOrder: 0,
            properties);

        return Product.Rehydrate(
            LaptopProductId,
            product.TemplateKey,
            product.Sku,
            product.Name,
            product.CategoryId,
            product.SortOrder,
            ProductStatus.Published,
            DateTimeOffset.UtcNow,
            product.CreatedAt,
            product.UpdatedAt,
            product.Properties);
    }

    private static Product BuildPhoneProduct()
    {
        var properties = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["name"] = "Lumen Phone X",
            ["sku"] = "LUM-PHX-128",
            ["shortDescription"] = "Flagship smartphone with variant support.",
            ["description"] = "<p>128GB base model. Add variants for storage and color options.</p>",
            ["price"] = 899.00m,
            ["compareAtPrice"] = 999.00m,
            ["isActive"] = true,
            ["category"] = "electronics"
        };

        var product = Product.Create(
            TemplateKeys.StandardProduct,
            "LUM-PHX-128",
            "Lumen Phone X",
            SmartphonesId,
            sortOrder: 0,
            properties);

        return Product.Rehydrate(
            PhoneProductId,
            product.TemplateKey,
            product.Sku,
            product.Name,
            product.CategoryId,
            product.SortOrder,
            ProductStatus.Published,
            DateTimeOffset.UtcNow,
            product.CreatedAt,
            product.UpdatedAt,
            product.Properties);
    }

    private static Product BuildHeadphonesProduct()
    {
        var properties = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["name"] = "Lumen Wireless Headphones",
            ["sku"] = "LUM-HP-WL",
            ["shortDescription"] = "Noise-cancelling over-ear headphones.",
            ["description"] = "<p>Comfortable fit with 30-hour battery life.</p>",
            ["price"] = 249.00m,
            ["isActive"] = true,
            ["category"] = "electronics"
        };

        var product = Product.Create(
            TemplateKeys.StandardProduct,
            "LUM-HP-WL",
            "Lumen Wireless Headphones",
            ElectronicsId,
            sortOrder: 20,
            properties);

        return Product.Rehydrate(
            HeadphonesProductId,
            product.TemplateKey,
            product.Sku,
            product.Name,
            product.CategoryId,
            product.SortOrder,
            ProductStatus.Draft,
            publishedAt: null,
            product.CreatedAt,
            product.UpdatedAt,
            product.Properties);
    }

    private static ProductVariant BuildPhoneVariant()
    {
        var properties = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["storage"] = "256GB",
            ["color"] = "Midnight Blue",
            ["price"] = 999.00m
        };

        var variant = ProductVariant.Create(
            PhoneProductId,
            "LUM-PHX-256-BLU",
            "Lumen Phone X — 256GB Midnight Blue",
            sortOrder: 10,
            properties);

        return ProductVariant.Rehydrate(
            PhoneVariantId,
            variant.ProductId,
            variant.Sku,
            variant.Name,
            variant.SortOrder,
            ProductStatus.Published,
            variant.CreatedAt,
            variant.UpdatedAt,
            variant.Properties);
    }

    private static CategoryEntity ToEntity(Category category) =>
        new()
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            ParentId = category.ParentId,
            SortOrder = category.SortOrder,
            Level = category.Level,
            MaterializedPath = category.MaterializedPath,
            FullPath = category.FullPath,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };

    private static ProductEntity ToEntity(Product product) =>
        new()
        {
            Id = product.Id,
            TemplateKey = product.TemplateKey,
            Sku = product.Sku,
            Name = product.Name,
            CategoryId = product.CategoryId,
            SortOrder = product.SortOrder,
            Status = product.Status,
            PublishedAt = product.PublishedAt,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            PropertiesJson = System.Text.Json.JsonSerializer.Serialize(product.Properties)
        };

    private static ProductVariantEntity ToEntity(ProductVariant variant) =>
        new()
        {
            Id = variant.Id,
            ProductId = variant.ProductId,
            Sku = variant.Sku,
            Name = variant.Name,
            SortOrder = variant.SortOrder,
            Status = variant.Status,
            CreatedAt = variant.CreatedAt,
            UpdatedAt = variant.UpdatedAt,
            PropertiesJson = System.Text.Json.JsonSerializer.Serialize(variant.Properties)
        };
}