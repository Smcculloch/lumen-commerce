using System.Text.Json;
using Lumen.Application.Content.Dtos;

namespace Lumen.Backoffice.Components.Content;

internal static class ContentPreviewHelper
{
    public static string GetTitle(ContentDto content) =>
        GetString(content, "title")
        ?? GetString(content, "headline")
        ?? content.Name;

    public static string? GetString(ContentDto content, string key)
    {
        if (!content.Properties.TryGetValue(key, out var value) || value is null)
        {
            return null;
        }

        return value switch
        {
            string s => s,
            JsonElement json when json.ValueKind == JsonValueKind.String => json.GetString(),
            JsonElement json when json.ValueKind == JsonValueKind.True => "Yes",
            JsonElement json when json.ValueKind == JsonValueKind.False => "No",
            bool b => b ? "Yes" : "No",
            _ => value.ToString()
        };
    }

    public static IEnumerable<(string Key, string Value)> GetDisplayProperties(ContentDto content)
    {
        foreach (var (key, value) in content.Properties.OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase))
        {
            if (IsRenderedInPreview(key))
            {
                continue;
            }

            var display = FormatValue(value);
            if (!string.IsNullOrWhiteSpace(display))
            {
                yield return (key, display);
            }
        }
    }

    private static bool IsRenderedInPreview(string key) =>
        string.Equals(key, "title", StringComparison.OrdinalIgnoreCase)
        || string.Equals(key, "slug", StringComparison.OrdinalIgnoreCase)
        || string.Equals(key, "headline", StringComparison.OrdinalIgnoreCase)
        || string.Equals(key, "introduction", StringComparison.OrdinalIgnoreCase)
        || string.Equals(key, "body", StringComparison.OrdinalIgnoreCase);

    private static string? FormatValue(object? value)
    {
        if (value is null)
        {
            return null;
        }

        return value switch
        {
            string s => s,
            bool b => b ? "Yes" : "No",
            JsonElement json when json.ValueKind == JsonValueKind.String => json.GetString(),
            JsonElement json when json.ValueKind == JsonValueKind.True => "Yes",
            JsonElement json when json.ValueKind == JsonValueKind.False => "No",
            JsonElement json when json.ValueKind == JsonValueKind.Array =>
                string.Join(", ", json.EnumerateArray().Select(e => e.ToString())),
            _ => value.ToString()
        };
    }
}