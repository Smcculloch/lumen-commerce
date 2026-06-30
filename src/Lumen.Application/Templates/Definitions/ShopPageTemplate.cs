using Lumen.Application.Templates.Attributes;
using Lumen.Domain.Enums;

namespace Lumen.Application.Templates.Definitions;

/// <summary>
/// CMS shop landing page with editorial content and an embedded product catalog block.
/// </summary>
[ContentTemplate("shop-page", "Shop Page", Description = "Merchandising landing page that renders editorial content above the PIM product catalog.")]
public sealed class ShopPageTemplate
{
    [TemplateProperty(PropertyType.Text, DisplayName = "Page Title", IsRequired = true, SortOrder = 10, MaxLength = 200)]
    public string Title { get; set; } = string.Empty;

    [TemplateProperty(PropertyType.Text, DisplayName = "URL Slug", IsRequired = true, SortOrder = 20, Pattern = "^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    public string Slug { get; set; } = string.Empty;

    [TemplateProperty(PropertyType.RichText, DisplayName = "Introduction", SortOrder = 30)]
    public string? Introduction { get; set; }

    [TemplateProperty(PropertyType.RichText, DisplayName = "Body", SortOrder = 40)]
    public string? Body { get; set; }

    [TemplateProperty(PropertyType.Boolean, DisplayName = "Show in Navigation", SortOrder = 50, DefaultValue = "true")]
    public bool ShowInNavigation { get; set; } = true;
}