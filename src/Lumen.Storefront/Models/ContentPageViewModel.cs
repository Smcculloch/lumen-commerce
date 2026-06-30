using System.Text.Json;
using Lumen.Application.Content.Dtos;

namespace Lumen.Storefront.Models;

public sealed class ContentPageViewModel
{
    public required string Title { get; init; }
    public string? Introduction { get; init; }
    public string? Body { get; init; }
    public required string TemplateKey { get; init; }
    public required string FullPath { get; init; }
    public IReadOnlyDictionary<string, object?> Properties { get; init; } = new Dictionary<string, object?>();

    public static ContentPageViewModel From(ContentDto content)
    {
        var title = GetString(content.Properties, "title")
            ?? GetString(content.Properties, "headline")
            ?? content.Name;

        return new ContentPageViewModel
        {
            Title = title,
            Introduction = GetString(content.Properties, "introduction"),
            Body = GetString(content.Properties, "body"),
            TemplateKey = content.TemplateKey,
            FullPath = content.FullPath,
            Properties = content.Properties
        };
    }

    private static string? GetString(IReadOnlyDictionary<string, object?> properties, string key)
    {
        if (!properties.TryGetValue(key, out var value) || value is null)
        {
            return null;
        }

        return value switch
        {
            string s => s,
            JsonElement json when json.ValueKind == JsonValueKind.String => json.GetString(),
            _ => value.ToString()
        };
    }
}