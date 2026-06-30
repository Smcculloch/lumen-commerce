using System.Text.Json;
using Lumen.Domain.Enums;
using Lumen.Infrastructure.Persistence.Entities;
using Lumen.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace Lumen.Infrastructure.Persistence.Seed;

/// <summary>
/// Seeds an example template extension to demonstrate the hybrid model.
/// </summary>
public static class TemplateSeedData
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static async Task SeedAsync(AppDbContext dbContext, CancellationToken cancellationToken = default)
    {
        if (await dbContext.TemplateDefinitions.AnyAsync(cancellationToken))
        {
            return;
        }

        var standardPageExtension = new TemplateDefinitionEntity
        {
            Key = TemplateKeys.StandardPage,
            DisplayName = "Standard Page Extensions",
            Description = "Backoffice-managed fields extending the code-defined Standard Page template.",
            Kind = TemplateKind.Content,
            BaseTemplateKey = TemplateKeys.StandardPage,
            IsDynamic = false,
            Properties =
            [
                new PropertyDefinitionEntity
                {
                    Name = "seoKeywords",
                    DisplayName = "SEO Keywords",
                    Description = "Optional comma-separated keywords for search optimization.",
                    Type = PropertyType.Text,
                    SortOrder = 1000,
                    MaxLength = 500
                },
                new PropertyDefinitionEntity
                {
                    Name = "promoBanner",
                    DisplayName = "Promotional Banner",
                    Type = PropertyType.Boolean,
                    SortOrder = 1010,
                    DefaultValue = "false"
                }
            ]
        };

        var landingPageTemplate = new TemplateDefinitionEntity
        {
            Key = "landing-page",
            DisplayName = "Landing Page",
            Description = "Fully dynamic landing page template created only via backoffice metadata.",
            Kind = TemplateKind.Content,
            IsDynamic = true,
            Properties =
            [
                new PropertyDefinitionEntity
                {
                    Name = "headline",
                    DisplayName = "Headline",
                    Type = PropertyType.Text,
                    IsRequired = true,
                    SortOrder = 10
                },
                new PropertyDefinitionEntity
                {
                    Name = "ctaText",
                    DisplayName = "Call To Action",
                    Type = PropertyType.Text,
                    SortOrder = 20
                },
                new PropertyDefinitionEntity
                {
                    Name = "theme",
                    DisplayName = "Theme",
                    Type = PropertyType.Select,
                    SortOrder = 30,
                    OptionsJson = JsonSerializer.Serialize(
                        new[]
                        {
                            new { value = "light", label = "Light", sortOrder = 0 },
                            new { value = "dark", label = "Dark", sortOrder = 1 }
                        },
                        JsonOptions)
                }
            ]
        };

        dbContext.TemplateDefinitions.AddRange(standardPageExtension, landingPageTemplate);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}