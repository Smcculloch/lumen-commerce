using Microsoft.AspNetCore.Identity;

namespace Lumen.Infrastructure.Identity;

/// <summary>
/// Identity user for storefront authentication.
/// </summary>
public sealed class ApplicationUser : IdentityUser
{
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}