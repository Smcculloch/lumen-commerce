using Lumen.Application.Content.Dtos;
using Lumen.Application.Products.Dtos;

namespace Lumen.Storefront.Models;

public sealed class ShopPageViewModel
{
    public required ContentPageViewModel Content { get; init; }
    public IReadOnlyList<ProductDetailViewModel> Products { get; init; } = [];

    public static ShopPageViewModel From(ContentDto content, IReadOnlyList<ProductDto> products) =>
        new()
        {
            Content = ContentPageViewModel.From(content),
            Products = products.Select(ProductDetailViewModel.From).ToList()
        };
}