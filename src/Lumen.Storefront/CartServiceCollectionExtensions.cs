using Lumen.Application.Cart;
using Lumen.Storefront.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Lumen.Storefront;

public static class CartServiceCollectionExtensions
{
    /// <summary>
    /// Registers cart services for the storefront host. Both ICartService and
    /// ICartContextProvider must be registered together — CartService depends on HTTP session context.
    /// </summary>
    public static IServiceCollection AddLumenStorefrontCart(this IServiceCollection services)
    {
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<ICartContextProvider, HttpCartContextProvider>();
        return services;
    }
}