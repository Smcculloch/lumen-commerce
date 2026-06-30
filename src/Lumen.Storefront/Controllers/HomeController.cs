using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Lumen.Storefront.Models;

namespace Lumen.Storefront.Controllers;

/// <summary>
/// Scaffold utilities only. CMS pages (including Privacy) are rendered by <see cref="ContentController"/>.
/// </summary>
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    [Route("Home/[action]")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
