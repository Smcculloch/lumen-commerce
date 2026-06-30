using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Lumen.Application.Cart.Dtos;
using Lumen.Application.Content.Dtos;

namespace Lumen.Storefront.Models;

public sealed class CheckoutPageViewModel
{
    public required ContentPageViewModel Content { get; init; }
    public string? TermsAndPolicies { get; init; }
    public required CartPageViewModel Cart { get; init; }
    public PlaceOrderFormModel Form { get; init; } = new();
    public bool IsAuthenticated { get; init; }

    public static CheckoutPageViewModel From(
        ContentDto content,
        CartDto cart,
        PlaceOrderFormModel? form = null,
        bool isAuthenticated = false) =>
        new()
        {
            Content = ContentPageViewModel.From(content),
            TermsAndPolicies = GetString(content.Properties, "termsAndPolicies"),
            Cart = CartPageViewModel.From(cart),
            Form = form ?? new PlaceOrderFormModel(),
            IsAuthenticated = isAuthenticated
        };

    private static string? GetString(IReadOnlyDictionary<string, object?> properties, string key)
    {
        if (!properties.TryGetValue(key, out var value) || value is null)
        {
            return null;
        }

        return value switch
        {
            string s => s,
            JsonElement json when json.ValueKind == JsonValueKind.String => json.GetString(),
            _ => value.ToString()
        };
    }
}

public sealed class PlaceOrderFormModel
{
    [Required]
    [StringLength(256)]
    [Display(Name = "Full name")]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(256)]
    [Display(Name = "Recipient name")]
    public string ShippingName { get; set; } = string.Empty;

    [Required]
    [StringLength(256)]
    [Display(Name = "Address line 1")]
    public string ShippingLine1 { get; set; } = string.Empty;

    [StringLength(256)]
    [Display(Name = "Address line 2")]
    public string? ShippingLine2 { get; set; }

    [Required]
    [StringLength(128)]
    public string ShippingCity { get; set; } = string.Empty;

    [StringLength(128)]
    [Display(Name = "State / region")]
    public string? ShippingRegion { get; set; }

    [Required]
    [StringLength(32)]
    [Display(Name = "Postal code")]
    public string ShippingPostalCode { get; set; } = string.Empty;

    [Required]
    [StringLength(128)]
    public string ShippingCountry { get; set; } = "Sweden";

    public bool BillingSameAsShipping { get; set; } = true;

    [StringLength(256)]
    [Display(Name = "Billing name")]
    public string? BillingName { get; set; }

    [StringLength(256)]
    [Display(Name = "Billing address line 1")]
    public string? BillingLine1 { get; set; }

    [StringLength(256)]
    [Display(Name = "Billing address line 2")]
    public string? BillingLine2 { get; set; }

    [StringLength(128)]
    [Display(Name = "Billing city")]
    public string? BillingCity { get; set; }

    [StringLength(128)]
    [Display(Name = "Billing state / region")]
    public string? BillingRegion { get; set; }

    [StringLength(32)]
    [Display(Name = "Billing postal code")]
    public string? BillingPostalCode { get; set; }

    [StringLength(128)]
    [Display(Name = "Billing country")]
    public string? BillingCountry { get; set; }

    [StringLength(2000)]
    [Display(Name = "Order notes")]
    public string? OrderNotes { get; set; }
}

public sealed class OrderConfirmationViewModel
{
    public required string OrderNumber { get; init; }
    public required string CustomerName { get; init; }
    public required string Email { get; init; }
    public decimal Subtotal { get; init; }
    public IReadOnlyList<CartLineViewModel> Items { get; init; } = [];

    public static OrderConfirmationViewModel From(Lumen.Application.Orders.Dtos.OrderDto order) =>
        new()
        {
            OrderNumber = order.OrderNumber,
            CustomerName = order.CustomerName,
            Email = order.Email,
            Subtotal = order.Subtotal,
            Items = order.Items
                .Select(i => new CartLineViewModel
                {
                    Id = i.Id,
                    Sku = i.Sku,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    LineTotal = i.LineTotal
                })
                .ToList()
        };
}

