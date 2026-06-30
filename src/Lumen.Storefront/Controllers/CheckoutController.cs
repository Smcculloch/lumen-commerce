using Microsoft.AspNetCore.Mvc;

namespace Lumen.Storefront.Controllers;

public class CheckoutController : Controller
{
    [HttpGet("/checkout")]
    public IActionResult Index()
    {
        ViewData["Title"] = "Checkout";
        return View();
    }
}