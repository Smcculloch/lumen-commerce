using Lumen.Application.Templates.Attributes;
using Lumen.Domain.Enums;

namespace Lumen.Application.Templates.Definitions;

/// <summary>
/// Attribute-driven example of an article/blog content template.
/// </summary>
[ContentTemplate("article-page", "Article Page", Description = "Editorial article with author, publish date, and tags.")]
public sealed class ArticlePageTemplate
{
    [TemplateProperty(PropertyType.Text, DisplayName = "Headline", IsRequired = true, SortOrder = 10)]
    public string Headline { get; set; } = string.Empty;

    [TemplateProperty(PropertyType.Text, DisplayName = "URL Slug", IsRequired = true, SortOrder = 20)]
    public string Slug { get; set; } = string.Empty;

    [TemplateProperty(PropertyType.Text, DisplayName = "Author", SortOrder = 30)]
    public string? Author { get; set; }

    [TemplateProperty(PropertyType.DateTime, DisplayName = "Publish Date", SortOrder = 40)]
    public DateTimeOffset? PublishDate { get; set; }

    [TemplateProperty(PropertyType.Image, DisplayName = "Featured Image", SortOrder = 50)]
    public string? FeaturedImage { get; set; }

    [TemplateProperty(PropertyType.RichText, DisplayName = "Article Body", IsRequired = true, SortOrder = 60)]
    public string Body { get; set; } = string.Empty;

    [TemplateProperty(
        PropertyType.MultiSelect,
        DisplayName = "Tags",
        SortOrder = 70,
        SelectOptions = "news:News,product:Product,company:Company")]
    public IReadOnlyList<string>? Tags { get; set; }
}