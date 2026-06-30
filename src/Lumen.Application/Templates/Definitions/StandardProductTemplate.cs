using Lumen.Application.Templates.Builders;
using Lumen.Domain.Enums;
using Lumen.Domain.Templates;

namespace Lumen.Application.Templates.Definitions;

/// <summary>
/// Fluent-builder example of a standard PIM product template.
/// </summary>
public static class StandardProductTemplate
{
    public static ProductTemplateDefinition Create() =>
        TemplateBuilder<ProductTemplateDefinition>
            .ForProduct("standard-product", "Standard Product")
            .WithDescription("Base commerce product with pricing, media, and merchandising fields.")
            .AddProperty("name", "Product Name", PropertyType.Text, p => p.Required().SortOrder(10).MaxLength(200))
            .AddProperty("sku", "SKU", PropertyType.Text, p => p.Required().SortOrder(20))
            .AddProperty("shortDescription", "Short Description", PropertyType.Text, p => p.SortOrder(30).MaxLength(500))
            .AddProperty("description", "Description", PropertyType.RichText, p => p.SortOrder(40))
            .AddProperty("price", "Price", PropertyType.Decimal, p => p.Required().SortOrder(50).MinValue(0))
            .AddProperty("compareAtPrice", "Compare At Price", PropertyType.Decimal, p => p.SortOrder(60).MinValue(0))
            .AddProperty("isActive", "Active", PropertyType.Boolean, p => p.SortOrder(70).DefaultValue("true"))
            .AddProperty("primaryImage", "Primary Image", PropertyType.Image, p => p.SortOrder(80))
            .AddProperty(
                "category",
                "Category",
                PropertyType.Select,
                p => p.SortOrder(90)
                    .AddOption("apparel", "Apparel")
                    .AddOption("electronics", "Electronics")
                    .AddOption("home", "Home & Garden"))
            .AddProperty("specifications", "Specifications", PropertyType.Json, p => p.SortOrder(100))
            .Build();
}