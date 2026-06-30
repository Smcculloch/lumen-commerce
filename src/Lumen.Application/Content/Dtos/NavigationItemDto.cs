namespace Lumen.Application.Content.Dtos;

/// <summary>
/// A published CMS page entry for storefront navigation.
/// </summary>
public sealed record NavigationItemDto(
    string Label,
    string Url,
    int Level);