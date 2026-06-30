using Lumen.Application.Content;
using Lumen.Application.Products;
using Lumen.Domain.Enums;
using Lumen.Shared.Constants;
using Lumen.Storefront.Models;
using Microsoft.AspNetCore.Mvc;

namespace Lumen.Storefront.Controllers;

/// <summary>
/// Renders published CMS content on the public storefront.
/// </summary>
public class ContentController : Controller
{
    private readonly IContentService _contentService;
    private readonly IProductService _productService;

    public ContentController(IContentService contentService, IProductService productService)
    {
        _contentService = contentService;
        _productService = productService;
    }

    [HttpGet("/")]
    public Task<IActionResult> Root(CancellationToken cancellationToken) =>
        RenderPublishedAsync("/home", cancellationToken);

    [HttpGet("/home")]
    public Task<IActionResult> Home(CancellationToken cancellationToken) =>
        RenderPublishedAsync("/home", cancellationToken);

    [HttpGet("/{**slug}")]
    public Task<IActionResult> Page(string slug, CancellationToken cancellationToken)
    {
        var fullPath = "/" + slug.TrimStart('/');
        return RenderPublishedAsync(fullPath, cancellationToken);
    }

    private async Task<IActionResult> RenderPublishedAsync(string fullPath, CancellationToken cancellationToken)
    {
        var content = await _contentService.GetContentBySlugAsync(fullPath.TrimStart('/'), cancellationToken);

        if (content is null || content.Status != ContentStatus.Published)
        {
            return NotFound();
        }

        if (string.Equals(content.TemplateKey, TemplateKeys.ShopPage, StringComparison.OrdinalIgnoreCase))
        {
            var products = await _productService.ListAsync(
                categoryId: null,
                status: ProductStatus.Published,
                search: null,
                cancellationToken);
            var shopModel = ShopPageViewModel.From(content, products);

            ViewData["Title"] = shopModel.Content.Title;
            return View("Shop", shopModel);
        }

        var model = ContentPageViewModel.From(content);
        ViewData["Title"] = model.Title;
        return View("Page", model);
    }
}