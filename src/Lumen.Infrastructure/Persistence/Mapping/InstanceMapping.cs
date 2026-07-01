using System.Text.Json;
using Lumen.Domain.Cart;
using Lumen.Domain.Categories;
using Lumen.Domain.Content;
using Lumen.Domain.Customers;
using Lumen.Domain.Media;
using Lumen.Domain.Orders;
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

    public static Order ToDomain(OrderEntity entity) =>
        Order.Rehydrate(
            entity.Id,
            entity.OrderNumber,
            entity.CustomerId,
            entity.CustomerName,
            entity.Email,
            ToDomainShippingAddress(entity),
            ToDomainBillingAddress(entity),
            entity.OrderNotes,
            entity.Status,
            entity.PaymentStatus,
            entity.PaymentProvider,
            entity.PaymentTransactionId,
            entity.PaymentMessage,
            entity.AmountCaptured,
            entity.AmountRefunded,
            entity.Subtotal,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.Items.Select(ToDomain));

    public static OrderLineItem ToDomain(OrderLineItemEntity entity) =>
        OrderLineItem.Rehydrate(
            entity.Id,
            entity.OrderId,
            entity.ProductId,
            entity.ProductVariantId,
            entity.Sku,
            entity.ProductName,
            entity.Quantity,
            entity.UnitPrice);

    public static OrderEntity ToEntity(Order order) =>
        new()
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId,
            CustomerName = order.CustomerName,
            Email = order.Email,
            ShippingName = order.ShippingAddress.Name,
            ShippingLine1 = order.ShippingAddress.Line1,
            ShippingLine2 = order.ShippingAddress.Line2,
            ShippingCity = order.ShippingAddress.City,
            ShippingRegion = order.ShippingAddress.Region,
            ShippingPostalCode = order.ShippingAddress.PostalCode,
            ShippingCountry = order.ShippingAddress.Country,
            BillingName = order.BillingAddress.Name,
            BillingLine1 = order.BillingAddress.Line1,
            BillingLine2 = order.BillingAddress.Line2,
            BillingCity = order.BillingAddress.City,
            BillingRegion = order.BillingAddress.Region,
            BillingPostalCode = order.BillingAddress.PostalCode,
            BillingCountry = order.BillingAddress.Country,
            OrderNotes = order.OrderNotes,
            Status = order.Status,
            PaymentStatus = order.PaymentStatus,
            PaymentProvider = order.PaymentProvider,
            PaymentTransactionId = order.PaymentTransactionId,
            PaymentMessage = order.PaymentMessage,
            AmountCaptured = order.AmountCaptured,
            AmountRefunded = order.AmountRefunded,
            Subtotal = order.Subtotal,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt
        };

    public static OrderLineItemEntity ToEntity(OrderLineItem item) =>
        new()
        {
            Id = item.Id,
            OrderId = item.OrderId,
            ProductId = item.ProductId,
            ProductVariantId = item.ProductVariantId,
            Sku = item.Sku,
            ProductName = item.ProductName,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice
        };

    private static OrderAddress ToDomainShippingAddress(OrderEntity entity) =>
        OrderAddress.Rehydrate(
            entity.ShippingName,
            entity.ShippingLine1,
            entity.ShippingLine2,
            entity.ShippingCity,
            entity.ShippingRegion,
            entity.ShippingPostalCode,
            entity.ShippingCountry);

    private static OrderAddress ToDomainBillingAddress(OrderEntity entity) =>
        OrderAddress.Rehydrate(
            entity.BillingName,
            entity.BillingLine1,
            entity.BillingLine2,
            entity.BillingCity,
            entity.BillingRegion,
            entity.BillingPostalCode,
            entity.BillingCountry);
}