using Lumen.Application.Products.Dtos;
using Lumen.Domain.Enums;
using Lumen.Shared.Extensions;

namespace Lumen.Storefront.Models;

public sealed class ProductDetailViewModel
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
    public string TemplateKey { get; init; } = string.Empty;
    public ProductStatus Status { get; init; }
    public string? ShortDescription { get; init; }
    public string? Description { get; init; }
    public decimal? Price { get; init; }
    public decimal? CompareAtPrice { get; init; }
    public IReadOnlyList<ProductVariantViewModel> Variants { get; init; } = [];

    public static ProductDetailViewModel From(ProductDto product)
    {
        product.Properties.TryGetValue("shortDescription", out var shortDesc);
        product.Properties.TryGetValue("description", out var desc);

        return new ProductDetailViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Sku = product.Sku,
            TemplateKey = product.TemplateKey,
            Status = product.Status,
            ShortDescription = shortDesc?.ToString(),
            Description = desc?.ToString(),
            Price = product.Properties.GetDecimal("price"),
            CompareAtPrice = product.Properties.GetDecimal("compareAtPrice"),
            Variants = product.Variants
                .Where(v => v.Status == ProductStatus.Published)
                .Select(ProductVariantViewModel.From)
                .ToList()
        };
    }
}

public sealed class ProductVariantViewModel
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;

    public static ProductVariantViewModel From(ProductVariantDto variant) =>
        new()
        {
            Id = variant.Id,
            Name = variant.Name,
            Sku = variant.Sku
        };
}