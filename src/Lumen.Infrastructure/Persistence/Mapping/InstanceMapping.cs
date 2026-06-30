using System.Text.Json;
using Lumen.Domain.Cart;
using Lumen.Domain.Categories;
using Lumen.Domain.Content;
using Lumen.Domain.Customers;
using Lumen.Domain.Media;
using Lumen.Domain.Products;
using Lumen.Infrastructure.Persistence.Entities;

namespace Lumen.Infrastructure.Persistence.Mapping;

internal static class InstanceMapping
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static ContentItem ToDomain(ContentItemEntity entity)
    {
        var properties = JsonSerializer.Deserialize<Dictionary<string, object?>>(entity.PropertiesJson, JsonOptions)
            ?? new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        return ContentItem.Rehydrate(
            entity.Id,
            entity.TemplateKey,
            entity.Name,
            entity.Slug,
            entity.ParentId,
            entity.SortOrder,
            entity.Level,
            entity.MaterializedPath,
            entity.FullPath,
            entity.Status,
            entity.PublishedAt,
            entity.CreatedAt,
            entity.UpdatedAt,
            properties);
    }

    public static ContentItemEntity ToEntity(ContentItem item) =>
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
            PropertiesJson = JsonSerializer.Serialize(item.Properties, JsonOptions)
        };

    public static MediaItem ToDomain(MediaItemEntity entity) =>
        MediaItem.Rehydrate(
            entity.Id,
            entity.FileName,
            entity.StoragePath,
            entity.PublicUrl,
            entity.MimeType,
            entity.SizeBytes,
            entity.AltText,
            entity.CreatedAt);

    public static MediaItemEntity ToEntity(MediaItem item) =>
        new()
        {
            Id = item.Id,
            FileName = item.FileName,
            StoragePath = item.StoragePath,
            PublicUrl = item.PublicUrl,
            MimeType = item.MimeType,
            SizeBytes = item.SizeBytes,
            AltText = item.AltText,
            CreatedAt = item.CreatedAt
        };

    public static Category ToDomain(CategoryEntity entity) =>
        Category.Rehydrate(
            entity.Id,
            entity.Name,
            entity.Slug,
            entity.ParentId,
            entity.SortOrder,
            entity.Level,
            entity.MaterializedPath,
            entity.FullPath,
            entity.CreatedAt,
            entity.UpdatedAt);

    public static CategoryEntity ToEntity(Category category) =>
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

    public static Product ToDomain(ProductEntity entity)
    {
        var properties = JsonSerializer.Deserialize<Dictionary<string, object?>>(entity.PropertiesJson, JsonOptions)
            ?? new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        return Product.Rehydrate(
            entity.Id,
            entity.TemplateKey,
            entity.Sku,
            entity.Name,
            entity.CategoryId,
            entity.SortOrder,
            entity.Status,
            entity.PublishedAt,
            entity.CreatedAt,
            entity.UpdatedAt,
            properties);
    }

    public static ProductEntity ToEntity(Product product) =>
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
            PropertiesJson = JsonSerializer.Serialize(product.Properties, JsonOptions)
        };

    public static ProductVariant ToDomain(ProductVariantEntity entity)
    {
        var properties = JsonSerializer.Deserialize<Dictionary<string, object?>>(entity.PropertiesJson, JsonOptions)
            ?? new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        return ProductVariant.Rehydrate(
            entity.Id,
            entity.ProductId,
            entity.Sku,
            entity.Name,
            entity.SortOrder,
            entity.Status,
            entity.CreatedAt,
            entity.UpdatedAt,
            properties);
    }

    public static ProductVariantEntity ToEntity(ProductVariant variant) =>
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
            PropertiesJson = JsonSerializer.Serialize(variant.Properties, JsonOptions)
        };

    public static Customer ToDomain(CustomerEntity entity) =>
        Customer.Rehydrate(
            entity.Id,
            entity.UserId,
            entity.Email,
            entity.DisplayName,
            entity.CreatedAt,
            entity.UpdatedAt);

    public static CustomerEntity ToEntity(Customer customer) =>
        new()
        {
            Id = customer.Id,
            UserId = customer.UserId,
            Email = customer.Email,
            DisplayName = customer.DisplayName,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt
        };

    public static ShoppingCart ToDomain(CartEntity entity) =>
        ShoppingCart.Rehydrate(
            entity.Id,
            entity.CustomerId,
            entity.SessionKey,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.Items.Select(ToDomain));

    public static CartItem ToDomain(CartItemEntity entity) =>
        CartItem.Rehydrate(
            entity.Id,
            entity.CartId,
            entity.ProductId,
            entity.ProductVariantId,
            entity.Sku,
            entity.ProductName,
            entity.Quantity,
            entity.UnitPrice,
            entity.CreatedAt,
            entity.UpdatedAt);

    public static CartEntity ToEntity(ShoppingCart cart) =>
        new()
        {
            Id = cart.Id,
            CustomerId = cart.CustomerId,
            SessionKey = cart.SessionKey,
            CreatedAt = cart.CreatedAt,
            UpdatedAt = cart.UpdatedAt
        };

    public static CartItemEntity ToEntity(CartItem item) =>
        new()
        {
            Id = item.Id,
            CartId = item.CartId,
            ProductId = item.ProductId,
            ProductVariantId = item.ProductVariantId,
            Sku = item.Sku,
            ProductName = item.ProductName,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt
        };
}