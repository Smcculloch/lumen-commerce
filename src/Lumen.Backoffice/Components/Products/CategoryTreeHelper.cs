using Lumen.Application.Categories.Dtos;

namespace Lumen.Backoffice.Components.Products;

internal static class CategoryTreeHelper
{
    public static async Task<Dictionary<Guid, List<CategoryDto>>> LoadChildrenLookupAsync(
        IReadOnlyList<CategoryDto> roots,
        Func<Guid, Task<IReadOnlyList<CategoryDto>>> getChildren)
    {
        var lookup = new Dictionary<Guid, List<CategoryDto>>();
        var queue = new Queue<CategoryDto>(roots.OrderBy(r => r.SortOrder).ThenBy(r => r.Name));

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

    public static IEnumerable<CategoryDto> FlattenAll(
        IReadOnlyList<CategoryDto> roots,
        IReadOnlyDictionary<Guid, List<CategoryDto>> childrenByParent)
    {
        foreach (var root in roots.OrderBy(r => r.SortOrder).ThenBy(r => r.Name))
        {
            foreach (var item in WalkAll(root, childrenByParent))
            {
                yield return item;
            }
        }
    }

    public static IEnumerable<CategoryDto> FlattenVisible(
        IReadOnlyList<CategoryDto> roots,
        IReadOnlyDictionary<Guid, List<CategoryDto>> childrenByParent,
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

    public static HashSet<Guid> GetExpandableIds(IReadOnlyDictionary<Guid, List<CategoryDto>> childrenByParent) =>
        childrenByParent.Where(kvp => kvp.Value.Count > 0).Select(kvp => kvp.Key).ToHashSet();

    public static HashSet<Guid> GetExpandedIdsForSearch(
        IReadOnlyList<CategoryDto> roots,
        IReadOnlyDictionary<Guid, List<CategoryDto>> childrenByParent,
        string search)
    {
        var expanded = new HashSet<Guid>();
        foreach (var root in roots)
        {
            CollectExpandedForSearch(root, childrenByParent, search, [], expanded);
        }

        return expanded;
    }

    private static void CollectExpandedForSearch(
        CategoryDto node,
        IReadOnlyDictionary<Guid, List<CategoryDto>> childrenByParent,
        string search,
        List<CategoryDto> ancestors,
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

    private static bool IsVisibleInTree(
        CategoryDto node,
        IReadOnlyDictionary<Guid, List<CategoryDto>> childrenByParent,
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

    private static IEnumerable<CategoryDto> WalkAll(
        CategoryDto node,
        IReadOnlyDictionary<Guid, List<CategoryDto>> childrenByParent)
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

    private static IEnumerable<CategoryDto> Walk(
        CategoryDto node,
        IReadOnlyDictionary<Guid, List<CategoryDto>> childrenByParent,
        ISet<Guid> expandedIds,
        string? search,
        List<CategoryDto> ancestors)
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

        var path = new List<CategoryDto>(ancestors) { node };
        foreach (var child in children)
        {
            foreach (var item in Walk(child, childrenByParent, expandedIds, search, path))
            {
                yield return item;
            }
        }
    }

    private static bool MatchesSearch(CategoryDto item, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return true;
        }

        return item.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
            || item.Slug.Contains(search, StringComparison.OrdinalIgnoreCase)
            || item.FullPath.Contains(search, StringComparison.OrdinalIgnoreCase);
    }
}