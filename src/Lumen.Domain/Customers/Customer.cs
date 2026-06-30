namespace Lumen.Domain.Customers;

/// <summary>
/// Lightweight commerce customer profile linked to an Identity user.
/// </summary>
public sealed class Customer
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string UserId { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? DisplayName { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public static Customer Create(string userId, string email, string? displayName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        return new Customer
        {
            UserId = userId,
            Email = email.Trim(),
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim()
        };
    }

    public static Customer Rehydrate(
        Guid id,
        string userId,
        string email,
        string? displayName,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt) =>
        new()
        {
            Id = id,
            UserId = userId,
            Email = email,
            DisplayName = displayName,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

    public void UpdateProfile(string email, string? displayName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        Email = email.Trim();
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}