using Lumen.Application.Cart.Dtos;

namespace Lumen.Application.Cart;

public interface ICartService
{
    Task<CartDto> GetCartAsync(CancellationToken cancellationToken = default);
    Task<CartDto> AddItemAsync(AddToCartRequest request, CancellationToken cancellationToken = default);
    Task<CartDto> UpdateQuantityAsync(Guid lineItemId, int quantity, CancellationToken cancellationToken = default);
    Task<CartDto> RemoveItemAsync(Guid lineItemId, CancellationToken cancellationToken = default);
    Task ClearCartAsync(CancellationToken cancellationToken = default);
    Task MergeAnonymousCartOnLoginAsync(string sessionKey, Guid customerId, CancellationToken cancellationToken = default);
}