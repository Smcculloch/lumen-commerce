using System.Security.Claims;
using Lumen.Application.Cart;
using Lumen.Application.Cart.Dtos;
using Lumen.Application.Customers;

namespace Lumen.Storefront.Services;

public sealed class HttpCartContextProvider : ICartContextProvider
{
    public const string SessionCartKey = "LumenCartSessionKey";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICustomerService _customerService;

    public HttpCartContextProvider(
        IHttpContextAccessor httpContextAccessor,
        ICustomerService customerService)
    {
        _httpContextAccessor = httpContextAccessor;
        _customerService = customerService;
    }

    public async Task<CartContext> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("HTTP context is not available.");

        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(userId))
            {
                var customer = await _customerService.GetByUserIdAsync(userId, cancellationToken);
                if (customer is not null)
                {
                    return new CartContext(null, customer.Id);
                }
            }
        }

        await httpContext.Session.LoadAsync(cancellationToken);
        var sessionKey = httpContext.Session.GetString(SessionCartKey);
        if (string.IsNullOrWhiteSpace(sessionKey))
        {
            sessionKey = Guid.NewGuid().ToString("N");
            httpContext.Session.SetString(SessionCartKey, sessionKey);
            await httpContext.Session.CommitAsync(cancellationToken);
        }

        return new CartContext(sessionKey, null);
    }

    public static string? GetSessionKeyFromHttpContext(HttpContext httpContext)
    {
        return httpContext.Session.GetString(SessionCartKey);
    }
}