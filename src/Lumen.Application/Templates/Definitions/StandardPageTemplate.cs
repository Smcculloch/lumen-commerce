using Lumen.Application.Templates.Attributes;
using Lumen.Domain.Enums;

namespace Lumen.Application.Templates.Definitions;

/// <summary>
/// Attribute-driven example of a standard CMS page template.
/// </summary>
[ContentTemplate("standard-page", "Standard Page", Description = "A general-purpose content page with hero and body content.")]
public sealed class StandardPageTemplate
{
    [TemplateProperty(PropertyType.Text, DisplayName = "Page Title", IsRequired = true, SortOrder = 10, MaxLength = 200)]
    public string Title { get; set; } = string.Empty;

    [TemplateProperty(PropertyType.Text, DisplayName = "URL Slug", IsRequired = true, SortOrder = 20, Pattern = "^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    public string Slug { get; set; } = string.Empty;

    [TemplateProperty(PropertyType.RichText, DisplayName = "Introduction", SortOrder = 30)]
    public string? Introduction { get; set; }

    [TemplateProperty(PropertyType.Image, DisplayName = "Hero Image", SortOrder = 40)]
    public string? HeroImage { get; set; }

    [TemplateProperty(PropertyType.RichText, DisplayName = "Body", IsRequired = true, SortOrder = 50)]
    public string Body { get; set; } = string.Empty;

    [TemplateProperty(PropertyType.Boolean, DisplayName = "Show in Navigation", SortOrder = 60, DefaultValue = "true")]
    public bool ShowInNavigation { get; set; } = true;
}