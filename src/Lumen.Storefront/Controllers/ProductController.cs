using Lumen.Application.Products;
using Lumen.Domain.Enums;
using Lumen.Storefront.Models;
using Microsoft.AspNetCore.Mvc;

namespace Lumen.Storefront.Controllers;

/// <summary>
/// Renders published products on the public storefront.
/// </summary>
public class ProductController : Controller
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet("/products")]
    public IActionResult Index() => RedirectPermanent("/shop");

    [HttpGet("/products/{sku}")]
    public async Task<IActionResult> Detail(string sku, CancellationToken cancellationToken)
    {
        var product = await _productService.GetBySkuAsync(sku, cancellationToken);

        if (product is null || product.Status != ProductStatus.Published)
        {
            return NotFound();
        }

        var model = ProductDetailViewModel.From(product);
        ViewData["Title"] = model.Name;
        return View("Detail", model);
    }
}