namespace Lumen.Application.Cart.Dtos;

public sealed record CartItemDto(
    Guid Id,
    Guid ProductId,
    Guid? ProductVariantId,
    string Sku,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);

public sealed record CartDto(
    Guid Id,
    Guid? CustomerId,
    string? SessionKey,
    IReadOnlyList<CartItemDto> Items,
    int ItemCount,
    decimal Subtotal);

public sealed record AddToCartRequest(
    Guid ProductId,
    Guid? ProductVariantId,
    int Quantity);

public sealed record CartContext(
    string? SessionKey,
    Guid? CustomerId);