namespace Lumen.Domain.Orders;

/// <summary>
/// Shipping or billing address captured at checkout.
/// </summary>
public sealed class OrderAddress
{
    public string Name { get; private set; } = string.Empty;
    public string Line1 { get; private set; } = string.Empty;
    public string? Line2 { get; private set; }
    public string City { get; private set; } = string.Empty;
    public string? Region { get; private set; }
    public string PostalCode { get; private set; } = string.Empty;
    public string Country { get; private set; } = string.Empty;

    public static OrderAddress Create(
        string name,
        string line1,
        string? line2,
        string city,
        string? region,
        string postalCode,
        string country)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(line1);
        ArgumentException.ThrowIfNullOrWhiteSpace(city);
        ArgumentException.ThrowIfNullOrWhiteSpace(postalCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(country);

        return new OrderAddress
        {
            Name = name.Trim(),
            Line1 = line1.Trim(),
            Line2 = string.IsNullOrWhiteSpace(line2) ? null : line2.Trim(),
            City = city.Trim(),
            Region = string.IsNullOrWhiteSpace(region) ? null : region.Trim(),
            PostalCode = postalCode.Trim(),
            Country = country.Trim()
        };
    }

    public static OrderAddress Rehydrate(
        string name,
        string line1,
        string? line2,
        string city,
        string? region,
        string postalCode,
        string country) =>
        new()
        {
            Name = name,
            Line1 = line1,
            Line2 = line2,
            City = city,
            Region = region,
            PostalCode = postalCode,
            Country = country
        };
}