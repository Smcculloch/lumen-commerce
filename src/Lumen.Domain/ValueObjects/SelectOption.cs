namespace Lumen.Domain.ValueObjects;

/// <summary>
/// A single option for Select and MultiSelect property types.
/// </summary>
public sealed record SelectOption(string Value, string Label, int SortOrder = 0);