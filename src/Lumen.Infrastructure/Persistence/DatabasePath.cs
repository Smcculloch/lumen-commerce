namespace Lumen.Infrastructure.Persistence;

/// <summary>
/// Resolves the shared SQLite database path so Backoffice and Storefront always use the same file
/// regardless of the process working directory.
/// </summary>
public static class DatabasePath
{
    public const string FileName = "lumencommerce.db";
    public const string DataFolderName = "data";

    public static string Resolve(string contentRootPath)
    {
        var normalizedRoot = Path.GetFullPath(contentRootPath);

        foreach (var candidate in EnumerateCandidates(normalizedRoot))
        {
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return GetDefaultPath(normalizedRoot);
    }

    public static string GetDefaultPath(string contentRootPath)
    {
        var solutionRoot = Path.GetFullPath(Path.Combine(contentRootPath, "..", ".."));
        return Path.Combine(solutionRoot, DataFolderName, FileName);
    }

    private static IEnumerable<string> EnumerateCandidates(string contentRootPath)
    {
        yield return Path.Combine(contentRootPath, DataFolderName, FileName);

        var projectParent = Path.GetFullPath(Path.Combine(contentRootPath, ".."));
        yield return Path.Combine(projectParent, DataFolderName, FileName);

        var solutionRoot = Path.GetFullPath(Path.Combine(contentRootPath, "..", ".."));
        yield return Path.Combine(solutionRoot, DataFolderName, FileName);
        yield return Path.Combine(contentRootPath, FileName);
    }

    public static string ToConnectionString(string databasePath) =>
        $"Data Source={databasePath}";
}