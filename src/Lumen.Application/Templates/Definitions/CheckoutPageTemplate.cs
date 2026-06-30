using Lumen.Application.Templates.Attributes;
using Lumen.Domain.Enums;

namespace Lumen.Application.Templates.Definitions;

/// <summary>
/// CMS checkout page with editorial content rendered above the functional checkout form.
/// </summary>
[ContentTemplate("checkout-page", "Checkout Page", Description = "Checkout page with CMS-managed content alongside the order form.")]
public sealed class CheckoutPageTemplate
{
    [TemplateProperty(PropertyType.Text, DisplayName = "Page Title", IsRequired = true, SortOrder = 10, MaxLength = 200)]
    public string Title { get; set; } = string.Empty;

    [TemplateProperty(PropertyType.Text, DisplayName = "URL Slug", IsRequired = true, SortOrder = 20, Pattern = "^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    public string Slug { get; set; } = string.Empty;

    [TemplateProperty(PropertyType.RichText, DisplayName = "Introduction", SortOrder = 30)]
    public string? Introduction { get; set; }

    [TemplateProperty(PropertyType.RichText, DisplayName = "Body", SortOrder = 40)]
    public string? Body { get; set; }

    [TemplateProperty(PropertyType.RichText, DisplayName = "Terms & Policies", SortOrder = 50)]
    public string? TermsAndPolicies { get; set; }
}