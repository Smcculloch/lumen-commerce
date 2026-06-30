using Lumen.Domain.Content;

namespace Lumen.Application.Content;

/// <summary>
/// Builds and updates materialized and slug-based paths for the content tree.
/// </summary>
public static class ContentPathHelper
{
    public static string BuildMaterializedPath(Guid id, ContentItem? parent) =>
        parent is null ? $"/{id}/" : $"{parent.MaterializedPath}{id}/";

    public static string BuildFullPath(string slug, ContentItem? parent)
    {
        var normalizedSlug = slug.Trim().ToLowerInvariant();
        return parent is null ? $"/{normalizedSlug}" : $"{parent.FullPath.TrimEnd('/')}/{normalizedSlug}";
    }

    public static int BuildLevel(ContentItem? parent) => parent?.Level + 1 ?? 0;

    public static string GenerateSlug(string name)
    {
        var slug = name.Trim().ToLowerInvariant();
        slug = string.Join('-', slug.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries));
        return new string(slug.Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray());
    }

    public static string EnsureUniqueSlug(string baseSlug, IEnumerable<string> existingSlugs)
    {
        var slug = string.IsNullOrWhiteSpace(baseSlug) ? "content" : baseSlug;
        var taken = new HashSet<string>(existingSlugs, StringComparer.OrdinalIgnoreCase);

        if (!taken.Contains(slug))
        {
            return slug;
        }

        for (var i = 2; i < 1000; i++)
        {
            var candidate = $"{slug}-{i}";
            if (!taken.Contains(candidate))
            {
                return candidate;
            }
        }

        return $"{slug}-{Guid.NewGuid():N}"[..Math.Min(slug.Length + 33, 128)];
    }
}