namespace Lumen.Backoffice.Components.Shared;

internal static class TreeReorderHelper
{
    public static IReadOnlyList<T> GetSiblings<T>(
        T node,
        IReadOnlyList<T> roots,
        IReadOnlyDictionary<Guid, List<T>> childrenByParent,
        Func<T, Guid?> getParentId,
        Func<T, int> getSortOrder,
        Func<T, string> getName)
    {
        if (getParentId(node) is Guid parentId)
        {
            return childrenByParent.TryGetValue(parentId, out var children)
                ? Order(children, getSortOrder, getName)
                : [];
        }

        return Order(roots, getSortOrder, getName);
    }

    public static IReadOnlyList<Guid>? MoveUp<T>(
        IReadOnlyList<T> siblings,
        Guid id,
        Func<T, Guid> getId)
    {
        var orderedIds = siblings.Select(getId).ToList();
        var index = orderedIds.IndexOf(id);
        if (index <= 0)
        {
            return null;
        }

        (orderedIds[index - 1], orderedIds[index]) = (orderedIds[index], orderedIds[index - 1]);
        return orderedIds;
    }

    public static IReadOnlyList<Guid>? MoveDown<T>(
        IReadOnlyList<T> siblings,
        Guid id,
        Func<T, Guid> getId)
    {
        var orderedIds = siblings.Select(getId).ToList();
        var index = orderedIds.IndexOf(id);
        if (index < 0 || index >= orderedIds.Count - 1)
        {
            return null;
        }

        (orderedIds[index + 1], orderedIds[index]) = (orderedIds[index], orderedIds[index + 1]);
        return orderedIds;
    }

    public static IReadOnlyList<Guid>? ReorderAfterDrop<T>(
        IReadOnlyList<T> siblings,
        Guid draggedId,
        Guid targetId,
        Func<T, Guid> getId)
    {
        if (draggedId == targetId)
        {
            return null;
        }

        var orderedIds = siblings.Select(getId).ToList();
        var fromIndex = orderedIds.IndexOf(draggedId);
        var toIndex = orderedIds.IndexOf(targetId);
        if (fromIndex < 0 || toIndex < 0)
        {
            return null;
        }

        orderedIds.RemoveAt(fromIndex);
        orderedIds.Insert(toIndex, draggedId);
        return orderedIds;
    }

    public static (bool CanMoveUp, bool CanMoveDown) GetMoveAvailability<T>(
        IReadOnlyList<T> siblings,
        Guid id,
        Func<T, Guid> getId)
    {
        var index = siblings.Select(getId).ToList().IndexOf(id);
        return (index > 0, index >= 0 && index < siblings.Count - 1);
    }

    private static List<T> Order<T>(
        IEnumerable<T> nodes,
        Func<T, int> getSortOrder,
        Func<T, string> getName) =>
        nodes.OrderBy(getSortOrder).ThenBy(getName).ToList();
}