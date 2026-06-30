using Lumen.Application.Cart.Dtos;

namespace Lumen.Storefront.Models;

public sealed class CartPageViewModel
{
    public IReadOnlyList<CartLineViewModel> Items { get; init; } = [];
    public int ItemCount { get; init; }
    public decimal Subtotal { get; init; }

    public static CartPageViewModel From(CartDto cart) =>
        new()
        {
            Items = cart.Items.Select(CartLineViewModel.From).ToList(),
            ItemCount = cart.ItemCount,
            Subtotal = cart.Subtotal
        };
}

public sealed class CartLineViewModel
{
    public Guid Id { get; init; }
    public string Sku { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal LineTotal { get; init; }

    public static CartLineViewModel From(CartItemDto item) =>
        new()
        {
            Id = item.Id,
            Sku = item.Sku,
            ProductName = item.ProductName,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            LineTotal = item.LineTotal
        };
}