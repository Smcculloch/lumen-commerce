namespace Lumen.Application.Common;

internal static class TreeSiblingReorder
{
    public static void ValidateSiblingOrder(
        IReadOnlyCollection<Guid> existingIds,
        IReadOnlyList<Guid> orderedIds)
    {
        if (orderedIds.Count != existingIds.Count)
        {
            throw new InvalidOperationException("The reorder request must include every sibling under the parent.");
        }

        if (orderedIds.Distinct().Count() != orderedIds.Count)
        {
            throw new InvalidOperationException("The reorder request contains duplicate node ids.");
        }

        if (orderedIds.Any(id => !existingIds.Contains(id)))
        {
            throw new InvalidOperationException("The reorder request includes nodes that are not siblings.");
        }
    }

    public static int SortOrderForIndex(int index) => (index + 1) * 10;
}