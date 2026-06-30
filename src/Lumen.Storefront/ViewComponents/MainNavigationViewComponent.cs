using Lumen.Application.Content;
using Microsoft.AspNetCore.Mvc;

namespace Lumen.Storefront.ViewComponents;

public class MainNavigationViewComponent : ViewComponent
{
    private readonly IContentService _contentService;

    public MainNavigationViewComponent(IContentService contentService)
    {
        _contentService = contentService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var items = await _contentService.GetNavigationAsync();
        return View(items);
    }
}