using System.Globalization;
using System.Text.Json;
using Lumen.Domain.Enums;

namespace Lumen.Backoffice.Components.Content;

/// <summary>
/// Converts between template property values and form-friendly strings.
/// </summary>
internal static class PropertyValueConverter
{
    public static string ToFormString(object? value, PropertyType type)
    {
        if (value is null)
        {
            return string.Empty;
        }

        if (value is JsonElement json)
        {
            value = ConvertJsonElement(json);
        }

        return type switch
        {
            PropertyType.Boolean => value switch
            {
                bool b => b ? "true" : "false",
                _ => bool.TryParse(value.ToString(), out var parsed) && parsed ? "true" : "false"
            },
            PropertyType.MultiSelect => value switch
            {
                IEnumerable<object?> items => string.Join(", ", items.Select(x => x?.ToString()).Where(x => !string.IsNullOrWhiteSpace(x))),
                string s => s,
                _ => value.ToString() ?? string.Empty
            },
            PropertyType.DateTime => value switch
            {
                DateTimeOffset dto => dto.ToString("yyyy-MM-ddTHH:mm"),
                DateTime dt => dt.ToString("yyyy-MM-ddTHH:mm"),
                _ => DateTimeOffset.TryParse(value.ToString(), out var parsed)
                    ? parsed.ToString("yyyy-MM-ddTHH:mm")
                    : value.ToString() ?? string.Empty
            },
            _ => value.ToString() ?? string.Empty
        };
    }

    public static object? FromFormString(string? value, PropertyType type)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return type switch
        {
            PropertyType.Boolean => bool.TryParse(value, out var b) && b,
            PropertyType.Integer or PropertyType.Number => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i) ? i : value,
            PropertyType.Decimal => decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var d) ? d : value,
            PropertyType.DateTime => DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dt)
                ? dt
                : value,
            PropertyType.MultiSelect => value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToArray(),
            _ => value
        };
    }

    private static object? ConvertJsonElement(JsonElement json) => json.ValueKind switch
    {
        JsonValueKind.String => json.GetString(),
        JsonValueKind.Number when json.TryGetInt64(out var l) => l,
        JsonValueKind.Number => json.GetDecimal(),
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        JsonValueKind.Array => json.EnumerateArray().Select(e => e.ToString()).ToArray(),
        JsonValueKind.Object => json.GetRawText(),
        _ => json.ToString()
    };
}