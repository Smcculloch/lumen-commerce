using Lumen.Domain.Categories;

namespace Lumen.Application.Categories;

public static class CategoryPathHelper
{
    public static string BuildMaterializedPath(Guid id, Category? parent) =>
        parent is null ? $"/{id}/" : $"{parent.MaterializedPath}{id}/";

    public static string BuildFullPath(string slug, Category? parent)
    {
        var normalizedSlug = slug.Trim().ToLowerInvariant();
        return parent is null ? $"/{normalizedSlug}" : $"{parent.FullPath.TrimEnd('/')}/{normalizedSlug}";
    }

    public static int BuildLevel(Category? parent) => parent?.Level + 1 ?? 0;

    public static string GenerateSlug(string name)
    {
        var slug = name.Trim().ToLowerInvariant();
        slug = string.Join('-', slug.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries));
        return new string(slug.Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray());
    }

    public static string EnsureUniqueSlug(string baseSlug, IEnumerable<string> existingSlugs)
    {
        var slug = string.IsNullOrWhiteSpace(baseSlug) ? "category" : baseSlug;
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