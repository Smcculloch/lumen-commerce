using Lumen.Application.Cart;
using Lumen.Application.Cart.Dtos;
using Lumen.Storefront.Models;
using Microsoft.AspNetCore.Mvc;

namespace Lumen.Storefront.Controllers;

public class CartController : Controller
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet("/cart")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var cart = await _cartService.GetCartAsync(cancellationToken);
        return View(CartPageViewModel.From(cart));
    }

    [HttpPost("/cart/add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(
        Guid productId,
        Guid? productVariantId,
        int quantity = 1,
        string? returnUrl = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _cartService.AddItemAsync(
                new AddToCartRequest(productId, productVariantId, Math.Max(1, quantity)),
                cancellationToken);
            TempData["CartMessage"] = "Item added to cart.";
        }
        catch (Exception ex)
        {
            TempData["CartError"] = ex.Message;
        }

        return LocalRedirect(returnUrl ?? "/cart");
    }

    [HttpPost("/cart/update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(Guid lineItemId, int quantity, CancellationToken cancellationToken = default)
    {
        try
        {
            if (quantity < 1)
            {
                await _cartService.RemoveItemAsync(lineItemId, cancellationToken);
            }
            else
            {
                await _cartService.UpdateQuantityAsync(lineItemId, quantity, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            TempData["CartError"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("/cart/remove")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(Guid lineItemId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _cartService.RemoveItemAsync(lineItemId, cancellationToken);
        }
        catch (Exception ex)
        {
            TempData["CartError"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("/cart/clear")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clear(CancellationToken cancellationToken = default)
    {
        await _cartService.ClearCartAsync(cancellationToken);
        return RedirectToAction(nameof(Index));
    }
}