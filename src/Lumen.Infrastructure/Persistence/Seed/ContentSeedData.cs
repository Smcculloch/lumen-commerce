using Lumen.Domain.Content;
using Lumen.Domain.Enums;
using Lumen.Infrastructure.Persistence.Entities;
using Lumen.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace Lumen.Infrastructure.Persistence.Seed;

/// <summary>
/// Seeds a hierarchical demo site: Home (root) → Privacy (child).
/// </summary>
public static class ContentSeedData
{
    private static readonly Guid HomeId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid PrivacyId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid ShopId = Guid.Parse("33333333-3333-3333-3333-333333333331");
    private static readonly Guid CheckoutId = Guid.Parse("44444444-4444-4444-4444-444444444441");

    public static async Task SeedAsync(AppDbContext dbContext, CancellationToken cancellationToken = default)
    {
        if (!await dbContext.ContentItems.AnyAsync(cancellationToken))
        {
            dbContext.ContentItems.Add(CreateHomeEntity());
            dbContext.ContentItems.Add(CreatePrivacyEntity());
            dbContext.ContentItems.Add(CreateShopEntity());
            dbContext.ContentItems.Add(CreateCheckoutEntity());
            await dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        if (!await dbContext.ContentItems.AnyAsync(c => c.Id == HomeId, cancellationToken))
        {
            dbContext.ContentItems.Add(CreateHomeEntity());
        }

        if (!await dbContext.ContentItems.AnyAsync(c => c.Id == PrivacyId, cancellationToken))
        {
            dbContext.ContentItems.Add(CreatePrivacyEntity());
        }

        if (!await dbContext.ContentItems.AnyAsync(c => c.Id == ShopId, cancellationToken))
        {
            dbContext.ContentItems.Add(CreateShopEntity());
        }

        if (!await dbContext.ContentItems.AnyAsync(c => c.Id == CheckoutId, cancellationToken))
        {
            dbContext.ContentItems.Add(CreateCheckoutEntity());
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static ContentItemEntity CreateHomeEntity()
    {
        var home = BuildHome();
        return ToEntity(home);
    }

    private static ContentItemEntity CreatePrivacyEntity()
    {
        var privacy = BuildPrivacy();
        return ToEntity(privacy);
    }

    private static ContentItemEntity CreateShopEntity()
    {
        var shop = BuildShop();
        return ToEntity(shop);
    }

    private static ContentItemEntity CreateCheckoutEntity()
    {
        var checkout = BuildCheckout();
        return ToEntity(checkout);
    }

    private static ContentItem BuildHome()
    {
        var slug = "home";
        var properties = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["title"] = "Welcome to Lumen Commerce",
            ["slug"] = slug,
            ["introduction"] = "A modern CMS and commerce platform built on a hybrid template system.",
            ["body"] = "<p>This Home page is the site root. Visit the <a href=\"/shop\">Shop</a> to browse products seeded from the PIM catalog.</p>",
            ["showInNavigation"] = true,
            ["seoKeywords"] = "lumen, commerce, cms",
            ["promoBanner"] = false
        };

        var home = ContentItem.Create(
            TemplateKeys.StandardPage,
            "Home",
            slug,
            parentId: null,
            sortOrder: 0,
            level: 0,
            materializedPath: $"/{HomeId}/",
            fullPath: $"/{slug}",
            properties);

        return ContentItem.Rehydrate(
            HomeId,
            home.TemplateKey,
            home.Name,
            home.Slug,
            home.ParentId,
            home.SortOrder,
            home.Level,
            home.MaterializedPath,
            home.FullPath,
            ContentStatus.Published,
            DateTimeOffset.UtcNow,
            home.CreatedAt,
            home.UpdatedAt,
            home.Properties);
    }

    private static ContentItem BuildPrivacy()
    {
        var slug = "privacy";
        var properties = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["title"] = "Privacy Policy",
            ["slug"] = slug,
            ["introduction"] = "How we handle your data.",
            ["body"] = "<p>This Privacy page is seeded as a <strong>child of Home</strong> at <code>/home/privacy</code>.</p>",
            ["showInNavigation"] = true,
            ["seoKeywords"] = "privacy, policy",
            ["promoBanner"] = false
        };

        var privacy = ContentItem.Create(
            TemplateKeys.StandardPage,
            "Privacy",
            slug,
            parentId: HomeId,
            sortOrder: 10,
            level: 1,
            materializedPath: $"/{HomeId}/{PrivacyId}/",
            fullPath: $"/home/{slug}",
            properties);

        return ContentItem.Rehydrate(
            PrivacyId,
            privacy.TemplateKey,
            privacy.Name,
            privacy.Slug,
            privacy.ParentId,
            privacy.SortOrder,
            privacy.Level,
            privacy.MaterializedPath,
            privacy.FullPath,
            ContentStatus.Published,
            DateTimeOffset.UtcNow,
            privacy.CreatedAt,
            privacy.UpdatedAt,
            privacy.Properties);
    }

    private static ContentItem BuildShop()
    {
        var slug = "shop";
        var properties = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["title"] = "Shop",
            ["slug"] = slug,
            ["introduction"] = "Discover products from the Lumen catalog — merchandised here as a CMS page with live PIM data below.",
            ["body"] = "<p>This page is managed in the CMS. The product grid beneath it is rendered from published PIM records.</p>",
            ["showInNavigation"] = true
        };

        var shop = ContentItem.Create(
            TemplateKeys.ShopPage,
            "Shop",
            slug,
            parentId: null,
            sortOrder: 5,
            level: 0,
            materializedPath: $"/{ShopId}/",
            fullPath: $"/{slug}",
            properties);

        return ContentItem.Rehydrate(
            ShopId,
            shop.TemplateKey,
            shop.Name,
            shop.Slug,
            shop.ParentId,
            shop.SortOrder,
            shop.Level,
            shop.MaterializedPath,
            shop.FullPath,
            ContentStatus.Published,
            DateTimeOffset.UtcNow,
            shop.CreatedAt,
            shop.UpdatedAt,
            shop.Properties);
    }

    private static ContentItem BuildCheckout()
    {
        var slug = "checkout";
        var properties = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["title"] = "Checkout",
            ["slug"] = slug,
            ["introduction"] = "Review your cart and complete your order. Guest checkout is available — no account required.",
            ["body"] = "<p>Enter your shipping and billing details below. Payment processing will be added in a future phase.</p>",
            ["termsAndPolicies"] = "<p><strong>Terms:</strong> By placing your order you agree to our standard terms of sale. Orders are fulfilled after manual review in this demo environment.</p>"
        };

        var checkout = ContentItem.Create(
            TemplateKeys.CheckoutPage,
            "Checkout",
            slug,
            parentId: null,
            sortOrder: 6,
            level: 0,
            materializedPath: $"/{CheckoutId}/",
            fullPath: $"/{slug}",
            properties);

        return ContentItem.Rehydrate(
            CheckoutId,
            checkout.TemplateKey,
            checkout.Name,
            checkout.Slug,
            checkout.ParentId,
            checkout.SortOrder,
            checkout.Level,
            checkout.MaterializedPath,
            checkout.FullPath,
            ContentStatus.Published,
            DateTimeOffset.UtcNow,
            checkout.CreatedAt,
            checkout.UpdatedAt,
            checkout.Properties);
    }

    private static ContentItemEntity ToEntity(ContentItem item) =>
        new()
        {
            Id = item.Id,
            TemplateKey = item.TemplateKey,
            Name = item.Name,
            Slug = item.Slug!,
            ParentId = item.ParentId,
            SortOrder = item.SortOrder,
            Level = item.Level,
            MaterializedPath = item.MaterializedPath,
            FullPath = item.FullPath,
            Status = item.Status,
            PublishedAt = item.PublishedAt,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt,
            PropertiesJson = System.Text.Json.JsonSerializer.Serialize(item.Properties)
        };
}