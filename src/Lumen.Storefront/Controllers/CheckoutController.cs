using System.Security.Claims;
using Lumen.Application.Cart;
using Lumen.Application.Content;
using Lumen.Application.Customers;
using Lumen.Application.Orders;
using Lumen.Application.Orders.Dtos;
using Lumen.Application.Payments.Dtos;
using Lumen.Domain.Enums;
using Lumen.Shared.Constants;
using Lumen.Storefront.Models;
using Microsoft.AspNetCore.Mvc;

namespace Lumen.Storefront.Controllers;

public class CheckoutController : Controller
{
    private readonly ICartService _cartService;
    private readonly IContentService _contentService;
    private readonly IOrderService _orderService;
    private readonly ICustomerService _customerService;

    public CheckoutController(
        ICartService cartService,
        IContentService contentService,
        IOrderService orderService,
        ICustomerService customerService)
    {
        _cartService = cartService;
        _contentService = contentService;
        _orderService = orderService;
        _customerService = customerService;
    }

    [HttpGet("/checkout")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var cart = await _cartService.GetCartAsync(cancellationToken);
        if (cart.Items.Count == 0)
        {
            TempData["CartError"] = "Your cart is empty. Add products before checking out.";
            return RedirectToAction("Index", "Cart");
        }

        var content = await _contentService.GetContentBySlugAsync("checkout", cancellationToken);
        if (content is null || content.Status != ContentStatus.Published)
        {
            return NotFound();
        }

        if (!string.Equals(content.TemplateKey, TemplateKeys.CheckoutPage, StringComparison.OrdinalIgnoreCase))
        {
            return NotFound();
        }

        var form = await BuildPrefillFormAsync(cancellationToken);
        var model = CheckoutPageViewModel.From(
            content,
            cart,
            form,
            isAuthenticated: User.Identity?.IsAuthenticated == true);

        ViewData["Title"] = model.Content.Title;
        return View(model);
    }

    [HttpPost("/checkout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOrder(PlaceOrderFormModel form, CancellationToken cancellationToken)
    {
        var cart = await _cartService.GetCartAsync(cancellationToken);
        var content = await _contentService.GetContentBySlugAsync("checkout", cancellationToken);

        if (content is null || content.Status != ContentStatus.Published)
        {
            return NotFound();
        }

        if (cart.Items.Count == 0)
        {
            TempData["CartError"] = "Your cart is empty.";
            return RedirectToAction("Index", "Cart");
        }

        ValidateBillingFields(form);

        if (!ModelState.IsValid)
        {
            var invalidModel = CheckoutPageViewModel.From(
                content,
                cart,
                form,
                isAuthenticated: User.Identity?.IsAuthenticated == true);
            ViewData["Title"] = invalidModel.Content.Title;
            return View("Index", invalidModel);
        }

        var customerId = await ResolveCustomerIdAsync(cancellationToken);
        var shipping = new OrderAddressDto(
            form.ShippingName,
            form.ShippingLine1,
            form.ShippingLine2,
            form.ShippingCity,
            form.ShippingRegion,
            form.ShippingPostalCode,
            form.ShippingCountry);

        var billing = form.BillingSameAsShipping
            ? shipping
            : new OrderAddressDto(
                form.BillingName!,
                form.BillingLine1!,
                form.BillingLine2,
                form.BillingCity!,
                form.BillingRegion,
                form.BillingPostalCode!,
                form.BillingCountry!);

        try
        {
            var result = await _orderService.CheckoutWithPaymentAsync(
                cart,
                new PlaceOrderRequest(
                    form.CustomerName,
                    form.Email,
                    shipping,
                    billing,
                    form.OrderNotes,
                    customerId),
                new PaymentRequest(
                    CardholderName: form.CustomerName,
                    SimulateFailure: form.SimulatePaymentFailure),
                cancellationToken);

            if (!result.PaymentSucceeded)
            {
                ModelState.AddModelError(
                    string.Empty,
                    result.PaymentError ?? "Payment could not be processed. Please try again.");
                var paymentFailedModel = CheckoutPageViewModel.From(
                    content,
                    cart,
                    form,
                    isAuthenticated: User.Identity?.IsAuthenticated == true);
                ViewData["Title"] = paymentFailedModel.Content.Title;
                return View("Index", paymentFailedModel);
            }

            await _cartService.ClearCartAsync(cancellationToken);

            return RedirectToAction(nameof(Confirmation), new { orderNumber = result.Order.OrderNumber });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var errorModel = CheckoutPageViewModel.From(
                content,
                cart,
                form,
                isAuthenticated: User.Identity?.IsAuthenticated == true);
            ViewData["Title"] = errorModel.Content.Title;
            return View("Index", errorModel);
        }
    }

    [HttpGet("/checkout/confirmation/{orderNumber}")]
    public async Task<IActionResult> Confirmation(string orderNumber, CancellationToken cancellationToken)
    {
        var order = await _orderService.GetByOrderNumberAsync(orderNumber, cancellationToken);
        if (order is null)
        {
            return NotFound();
        }

        ViewData["Title"] = $"Order {order.OrderNumber}";
        return View(OrderConfirmationViewModel.From(order));
    }

    private async Task<PlaceOrderFormModel> BuildPrefillFormAsync(CancellationToken cancellationToken)
    {
        var form = new PlaceOrderFormModel();

        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(userId))
            {
                var customer = await _customerService.GetByUserIdAsync(userId, cancellationToken);
                if (customer is not null)
                {
                    form.CustomerName = customer.DisplayName ?? customer.Email;
                    form.Email = customer.Email;
                    form.ShippingName = form.CustomerName;
                }
            }
        }

        return form;
    }

    private async Task<Guid?> ResolveCustomerIdAsync(CancellationToken cancellationToken)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        var customer = await _customerService.GetByUserIdAsync(userId, cancellationToken);
        return customer?.Id;
    }

    private void ValidateBillingFields(PlaceOrderFormModel form)
    {
        if (form.BillingSameAsShipping)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(form.BillingName))
        {
            ModelState.AddModelError(nameof(form.BillingName), "Billing name is required.");
        }

        if (string.IsNullOrWhiteSpace(form.BillingLine1))
        {
            ModelState.AddModelError(nameof(form.BillingLine1), "Billing address is required.");
        }

        if (string.IsNullOrWhiteSpace(form.BillingCity))
        {
            ModelState.AddModelError(nameof(form.BillingCity), "Billing city is required.");
        }

        if (string.IsNullOrWhiteSpace(form.BillingPostalCode))
        {
            ModelState.AddModelError(nameof(form.BillingPostalCode), "Billing postal code is required.");
        }

        if (string.IsNullOrWhiteSpace(form.BillingCountry))
        {
            ModelState.AddModelError(nameof(form.BillingCountry), "Billing country is required.");
        }
    }
}