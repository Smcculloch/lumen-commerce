using Lumen.Application.Content.Dtos;
using Lumen.Domain.Content;
using Lumen.Domain.Enums;
using Lumen.Shared.Extensions;

namespace Lumen.Application.Content;

internal static class ContentNavigationBuilder
{
    public static async Task<IReadOnlyList<NavigationItemDto>> BuildAsync(
        IReadOnlyList<ContentItem> roots,
        Func<Guid, CancellationToken, Task<IReadOnlyList<ContentItem>>> getChildrenAsync,
        CancellationToken cancellationToken = default)
    {
        var items = new List<NavigationItemDto>();

        foreach (var root in roots.OrderBy(r => r.SortOrder).ThenBy(r => r.Name))
        {
            await AppendVisibleAsync(root, getChildrenAsync, items, cancellationToken);
        }

        return items;
    }

    private static async Task AppendVisibleAsync(
        ContentItem item,
        Func<Guid, CancellationToken, Task<IReadOnlyList<ContentItem>>> getChildrenAsync,
        List<NavigationItemDto> items,
        CancellationToken cancellationToken)
    {
        if (!IsVisibleInNavigation(item))
        {
            return;
        }

        items.Add(new NavigationItemDto(
            ResolveLabel(item),
            ResolveUrl(item),
            item.Level));

        var children = await getChildrenAsync(item.Id, cancellationToken);
        foreach (var child in children.OrderBy(c => c.SortOrder).ThenBy(c => c.Name))
        {
            await AppendVisibleAsync(child, getChildrenAsync, items, cancellationToken);
        }
    }

    private static bool IsVisibleInNavigation(ContentItem item)
    {
        if (item.Status != ContentStatus.Published)
        {
            return false;
        }

        return item.Properties.GetBoolean("showInNavigation") ?? true;
    }

    private static string ResolveLabel(ContentItem item) =>
        item.Properties.GetString("title") ?? item.Name;

    private static string ResolveUrl(ContentItem item) =>
        string.Equals(item.FullPath, "/home", StringComparison.OrdinalIgnoreCase)
            ? "/"
            : item.FullPath;
}