using System.Text.Json;

namespace Lumen.Shared.Extensions;

/// <summary>
/// Helpers for reading typed values from template property bags.
/// </summary>
public static class PropertyBagExtensions
{
    public static string? GetString(this IReadOnlyDictionary<string, object?> bag, string name) =>
        bag.TryGetValue(name, out var value) ? Convert.ToString(value) : null;

    public static int? GetInt32(this IReadOnlyDictionary<string, object?> bag, string name)
    {
        if (!bag.TryGetValue(name, out var value) || value is null)
        {
            return null;
        }

        return value switch
        {
            int i => i,
            long l => (int)l,
            decimal d => (int)d,
            double dbl => (int)dbl,
            string s when int.TryParse(s, out var parsed) => parsed,
            JsonElement json when json.ValueKind == JsonValueKind.Number => json.GetInt32(),
            _ => null
        };
    }

    public static decimal? GetDecimal(this IReadOnlyDictionary<string, object?> bag, string name)
    {
        if (!bag.TryGetValue(name, out var value) || value is null)
        {
            return null;
        }

        return value switch
        {
            decimal d => d,
            int i => i,
            long l => l,
            double dbl => (decimal)dbl,
            string s when decimal.TryParse(s, out var parsed) => parsed,
            JsonElement json when json.ValueKind == JsonValueKind.Number => json.GetDecimal(),
            _ => null
        };
    }

    public static bool? GetBoolean(this IReadOnlyDictionary<string, object?> bag, string name)
    {
        if (!bag.TryGetValue(name, out var value) || value is null)
        {
            return null;
        }

        return value switch
        {
            bool b => b,
            string s when bool.TryParse(s, out var parsed) => parsed,
            JsonElement json when json.ValueKind is JsonValueKind.True or JsonValueKind.False => json.GetBoolean(),
            _ => null
        };
    }

    public static DateTimeOffset? GetDateTimeOffset(this IReadOnlyDictionary<string, object?> bag, string name)
    {
        if (!bag.TryGetValue(name, out var value) || value is null)
        {
            return null;
        }

        return value switch
        {
            DateTimeOffset dto => dto,
            DateTime dt => new DateTimeOffset(dt),
            string s when DateTimeOffset.TryParse(s, out var parsed) => parsed,
            JsonElement json when json.ValueKind == JsonValueKind.String &&
                                  DateTimeOffset.TryParse(json.GetString(), out var parsed) => parsed,
            _ => null
        };
    }
}