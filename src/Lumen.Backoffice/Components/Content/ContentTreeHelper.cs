using Lumen.Application.Content.Dtos;

namespace Lumen.Backoffice.Components.Content;

internal static class ContentTreeHelper
{
    public static async Task<Dictionary<Guid, List<ContentDto>>> LoadChildrenLookupAsync(
        IReadOnlyList<ContentDto> roots,
        Func<Guid, Task<IReadOnlyList<ContentDto>>> getChildren)
    {
        var lookup = new Dictionary<Guid, List<ContentDto>>();
        var queue = new Queue<ContentDto>(roots.OrderBy(r => r.SortOrder).ThenBy(r => r.Name));

        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            var children = (await getChildren(node.Id)).OrderBy(c => c.SortOrder).ThenBy(c => c.Name).ToList();
            lookup[node.Id] = children;

            foreach (var child in children)
            {
                queue.Enqueue(child);
            }
        }

        return lookup;
    }

    public static IEnumerable<ContentDto> FlattenAll(
        IReadOnlyList<ContentDto> roots,
        IReadOnlyDictionary<Guid, List<ContentDto>> childrenByParent)
    {
        foreach (var root in roots.OrderBy(r => r.SortOrder).ThenBy(r => r.Name))
        {
            foreach (var item in WalkAll(root, childrenByParent))
            {
                yield return item;
            }
        }
    }

    public static IEnumerable<ContentDto> FlattenVisible(
        IReadOnlyList<ContentDto> roots,
        IReadOnlyDictionary<Guid, List<ContentDto>> childrenByParent,
        ISet<Guid> expandedIds,
        string? search)
    {
        foreach (var root in roots.OrderBy(r => r.SortOrder).ThenBy(r => r.Name))
        {
            foreach (var item in Walk(root, childrenByParent, expandedIds, search, ancestors: []))
            {
                yield return item;
            }
        }
    }

    public static HashSet<Guid> GetExpandableIds(IReadOnlyDictionary<Guid, List<ContentDto>> childrenByParent) =>
        childrenByParent.Where(kvp => kvp.Value.Count > 0).Select(kvp => kvp.Key).ToHashSet();

    public static HashSet<Guid> GetExpandedIdsForSearch(
        IReadOnlyList<ContentDto> roots,
        IReadOnlyDictionary<Guid, List<ContentDto>> childrenByParent,
        string search)
    {
        var expanded = new HashSet<Guid>();
        foreach (var root in roots)
        {
            CollectExpandedForSearch(root, childrenByParent, search, [], expanded);
        }

        return expanded;
    }

    public static bool IsVisibleInTree(
        ContentDto node,
        IReadOnlyDictionary<Guid, List<ContentDto>> childrenByParent,
        string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return true;
        }

        if (MatchesSearch(node, search))
        {
            return true;
        }

        return childrenByParent.TryGetValue(node.Id, out var children) &&
               children.Any(child => IsVisibleInTree(child, childrenByParent, search));
    }

    private static void CollectExpandedForSearch(
        ContentDto node,
        IReadOnlyDictionary<Guid, List<ContentDto>> childrenByParent,
        string search,
        List<ContentDto> ancestors,
        ISet<Guid> expanded)
    {
        if (!childrenByParent.TryGetValue(node.Id, out var children) || children.Count == 0)
        {
            return;
        }

        var subtreeMatches = children.Any(child => IsVisibleInTree(child, childrenByParent, search));
        if (MatchesSearch(node, search) || subtreeMatches)
        {
            expanded.Add(node.Id);
            foreach (var child in children)
            {
                CollectExpandedForSearch(child, childrenByParent, search, [..ancestors, node], expanded);
            }
        }
    }

    private static IEnumerable<ContentDto> WalkAll(
        ContentDto node,
        IReadOnlyDictionary<Guid, List<ContentDto>> childrenByParent)
    {
        yield return node;

        if (!childrenByParent.TryGetValue(node.Id, out var children))
        {
            yield break;
        }

        foreach (var child in children)
        {
            foreach (var item in WalkAll(child, childrenByParent))
            {
                yield return item;
            }
        }
    }

    private static IEnumerable<ContentDto> Walk(
        ContentDto node,
        IReadOnlyDictionary<Guid, List<ContentDto>> childrenByParent,
        ISet<Guid> expandedIds,
        string? search,
        List<ContentDto> ancestors)
    {
        var matches = MatchesSearch(node, search);
        var ancestorMatches = ancestors.Any(a => MatchesSearch(a, search));

        if (string.IsNullOrWhiteSpace(search) || matches || ancestorMatches)
        {
            yield return node;
        }

        if (!expandedIds.Contains(node.Id) || !childrenByParent.TryGetValue(node.Id, out var children))
        {
            yield break;
        }

        var path = new List<ContentDto>(ancestors) { node };
        foreach (var child in children)
        {
            foreach (var item in Walk(child, childrenByParent, expandedIds, search, path))
            {
                yield return item;
            }
        }
    }

    private static bool MatchesSearch(ContentDto item, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return true;
        }

        return item.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
            || item.Slug.Contains(search, StringComparison.OrdinalIgnoreCase)
            || item.FullPath.Contains(search, StringComparison.OrdinalIgnoreCase)
            || item.TemplateKey.Contains(search, StringComparison.OrdinalIgnoreCase);
    }
}