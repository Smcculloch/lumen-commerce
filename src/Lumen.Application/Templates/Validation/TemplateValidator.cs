using System.Text.Json;
using System.Text.RegularExpressions;
using Lumen.Domain.Exceptions;
using Lumen.Domain.Templates;

namespace Lumen.Application.Templates.Validation;

/// <summary>
/// Validates property bags against a resolved template definition.
/// </summary>
public sealed class TemplateValidator
{
    public void Validate(ResolvedTemplateDefinition template, IDictionary<string, object?> values)
    {
        var errors = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var property in template.Properties)
        {
            values.TryGetValue(property.Name, out var value);
            ValidateProperty(property, value, errors);
        }

        if (errors.Count > 0)
        {
            throw new TemplateValidationException(
                errors.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToArray(), StringComparer.OrdinalIgnoreCase));
        }
    }

    public IDictionary<string, object?> Hydrate(
        ResolvedTemplateDefinition template,
        IDictionary<string, object?> values,
        bool applyDefaults = true)
    {
        Validate(template, values);

        var hydrated = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        foreach (var property in template.Properties)
        {
            if (values.TryGetValue(property.Name, out var value) && value is not null)
            {
                hydrated[property.Name] = NormalizeValue(property.Type, value);
            }
            else if (applyDefaults && property.DefaultValue is not null)
            {
                hydrated[property.Name] = NormalizeDefault(property.Type, property.DefaultValue);
            }
            else
            {
                hydrated[property.Name] = null;
            }
        }

        return hydrated;
    }

    private static void ValidateProperty(
        PropertyDefinition property,
        object? value,
        IDictionary<string, List<string>> errors)
    {
        if (property.IsRequired && IsEmpty(value))
        {
            AddError(errors, property.Name, $"{property.DisplayName} is required.");
            return;
        }

        if (IsEmpty(value))
        {
            return;
        }

        switch (property.Type)
        {
            case Domain.Enums.PropertyType.Text:
            case Domain.Enums.PropertyType.RichText:
            case Domain.Enums.PropertyType.Html:
                ValidateText(property, Convert.ToString(value), errors);
                break;
            case Domain.Enums.PropertyType.Integer:
                if (!TryGetInt(value, out _))
                {
                    AddError(errors, property.Name, $"{property.DisplayName} must be an integer.");
                }
                else
                {
                    ValidateNumeric(property, TryGetInt(value, out var intValue) ? intValue : null, errors);
                }

                break;
            case Domain.Enums.PropertyType.Decimal:
            case Domain.Enums.PropertyType.Number:
                if (!TryGetDecimal(value, out _))
                {
                    AddError(errors, property.Name, $"{property.DisplayName} must be a number.");
                }
                else
                {
                    ValidateNumeric(property, TryGetDecimal(value, out var decimalValue) ? decimalValue : null, errors);
                }

                break;
            case Domain.Enums.PropertyType.Boolean:
                if (!TryGetBoolean(value, out _))
                {
                    AddError(errors, property.Name, $"{property.DisplayName} must be a boolean.");
                }

                break;
            case Domain.Enums.PropertyType.DateTime:
                if (!TryGetDateTime(value, out _))
                {
                    AddError(errors, property.Name, $"{property.DisplayName} must be a valid date/time.");
                }

                break;
            case Domain.Enums.PropertyType.Select:
                ValidateSelect(property, value, errors, multi: false);
                break;
            case Domain.Enums.PropertyType.MultiSelect:
                ValidateSelect(property, value, errors, multi: true);
                break;
        }
    }

    private static void ValidateText(PropertyDefinition property, string? text, IDictionary<string, List<string>> errors)
    {
        if (text is null)
        {
            return;
        }

        if (property.Validation.MinLength is int minLength && text.Length < minLength)
        {
            AddError(errors, property.Name, $"{property.DisplayName} must be at least {minLength} characters.");
        }

        if (property.Validation.MaxLength is int maxLength && text.Length > maxLength)
        {
            AddError(errors, property.Name, $"{property.DisplayName} must be at most {maxLength} characters.");
        }

        if (property.Validation.Pattern is string pattern &&
            !Regex.IsMatch(text, pattern, RegexOptions.CultureInvariant))
        {
            AddError(errors, property.Name, $"{property.DisplayName} has an invalid format.");
        }
    }

    private static void ValidateNumeric(
        PropertyDefinition property,
        decimal? numericValue,
        IDictionary<string, List<string>> errors)
    {
        if (numericValue is null)
        {
            return;
        }

        if (property.Validation.MinValue is decimal min && numericValue < min)
        {
            AddError(errors, property.Name, $"{property.DisplayName} must be at least {min}.");
        }

        if (property.Validation.MaxValue is decimal max && numericValue > max)
        {
            AddError(errors, property.Name, $"{property.DisplayName} must be at most {max}.");
        }
    }

    private static void ValidateSelect(
        PropertyDefinition property,
        object? value,
        IDictionary<string, List<string>> errors,
        bool multi)
    {
        var allowed = property.Options.Select(o => o.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (multi)
        {
            foreach (var item in ToEnumerable(value))
            {
                if (!allowed.Contains(Convert.ToString(item) ?? string.Empty))
                {
                    AddError(errors, property.Name, $"{property.DisplayName} contains an invalid option.");
                }
            }
        }
        else if (!allowed.Contains(Convert.ToString(value) ?? string.Empty))
        {
            AddError(errors, property.Name, $"{property.DisplayName} contains an invalid option.");
        }
    }

    private static IEnumerable<object?> ToEnumerable(object? value) => value switch
    {
        null => Array.Empty<object?>(),
        string s => s.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
        JsonElement json when json.ValueKind == JsonValueKind.Array =>
            json.EnumerateArray().Select(e => (object?)e.ToString()),
        IEnumerable<object?> enumerable => enumerable,
        _ => new[] { value }
    };

    private static object? NormalizeValue(Domain.Enums.PropertyType type, object value) => type switch
    {
        Domain.Enums.PropertyType.Integer => TryGetInt(value, out var i) ? i : value,
        Domain.Enums.PropertyType.Decimal or Domain.Enums.PropertyType.Number =>
            TryGetDecimal(value, out var d) ? d : value,
        Domain.Enums.PropertyType.Boolean => TryGetBoolean(value, out var b) ? b : value,
        Domain.Enums.PropertyType.DateTime => TryGetDateTime(value, out var dt) ? dt : value,
        Domain.Enums.PropertyType.MultiSelect => ToEnumerable(value).Select(v => Convert.ToString(v)).ToArray(),
        _ => value
    };

    private static object? NormalizeDefault(Domain.Enums.PropertyType type, string defaultValue) => type switch
    {
        Domain.Enums.PropertyType.Boolean => bool.Parse(defaultValue),
        Domain.Enums.PropertyType.Integer => int.Parse(defaultValue),
        Domain.Enums.PropertyType.Decimal or Domain.Enums.PropertyType.Number => decimal.Parse(defaultValue),
        Domain.Enums.PropertyType.DateTime => DateTimeOffset.Parse(defaultValue),
        _ => defaultValue
    };

    private static bool IsEmpty(object? value) =>
        value is null || (value is string s && string.IsNullOrWhiteSpace(s));

    private static void AddError(IDictionary<string, List<string>> errors, string key, string message)
    {
        if (!errors.TryGetValue(key, out var messages))
        {
            messages = [];
            errors[key] = messages;
        }

        messages.Add(message);
    }

    private static bool TryGetInt(object value, out int result)
    {
        result = 0;
        return value switch
        {
            int i => (result = i) == i,
            long l => (result = (int)l) == (int)l,
            decimal d => (result = (int)d) == (int)d,
            double dbl => (result = (int)dbl) == (int)dbl,
            string s => int.TryParse(s, out result),
            JsonElement json when json.ValueKind == JsonValueKind.Number => (result = json.GetInt32()) == json.GetInt32(),
            _ => false
        };
    }

    private static bool TryGetDecimal(object value, out decimal result)
    {
        result = 0;
        return value switch
        {
            decimal d => (result = d) == d,
            int i => (result = i) == i,
            long l => (result = l) == l,
            double dbl => (result = (decimal)dbl) == (decimal)dbl,
            string s => decimal.TryParse(s, out result),
            JsonElement json when json.ValueKind == JsonValueKind.Number => (result = json.GetDecimal()) == json.GetDecimal(),
            _ => false
        };
    }

    private static bool TryGetBoolean(object value, out bool result)
    {
        result = false;
        return value switch
        {
            bool b => (result = b) == b,
            string s => bool.TryParse(s, out result),
            JsonElement json when json.ValueKind is JsonValueKind.True or JsonValueKind.False =>
                (result = json.GetBoolean()) == json.GetBoolean(),
            _ => false
        };
    }

    private static bool TryGetDateTime(object value, out DateTimeOffset result)
    {
        result = default;
        return value switch
        {
            DateTimeOffset dto => (result = dto) == dto,
            DateTime dt => (result = new DateTimeOffset(dt)) == new DateTimeOffset(dt),
            string s => DateTimeOffset.TryParse(s, out result),
            JsonElement json when json.ValueKind == JsonValueKind.String &&
                                  DateTimeOffset.TryParse(json.GetString(), out result) => true,
            _ => false
        };
    }
}