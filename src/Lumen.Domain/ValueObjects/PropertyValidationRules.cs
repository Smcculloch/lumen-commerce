namespace Lumen.Domain.ValueObjects;

/// <summary>
/// Optional validation constraints for a template property.
/// </summary>
public sealed record PropertyValidationRules(
    int? MinLength = null,
    int? MaxLength = null,
    decimal? MinValue = null,
    decimal? MaxValue = null,
    string? Pattern = null)
{
    public static PropertyValidationRules None { get; } = new();
}