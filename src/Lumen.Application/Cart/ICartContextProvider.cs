using Lumen.Application.Cart.Dtos;

namespace Lumen.Application.Cart;

/// <summary>
/// Resolves the current cart identity from HTTP session and authenticated user.
/// </summary>
public interface ICartContextProvider
{
    Task<CartContext> GetCurrentAsync(CancellationToken cancellationToken = default);
}