namespace MobileShop.Dal.EfStructures;

/// <summary>
/// Resolves well-known paths relative to the solution root (found by walking up from the current
/// directory until a folder containing <c>src</c> is found), so the database file and log directory
/// live next to the solution rather than wherever the process happens to be running from.
/// </summary>
public static class SolutionPaths
{
    /// <summary>The solution root directory.</summary>
    public static string Root { get; } = FindRoot();

    /// <summary>Full path to the SQLite database file, at the solution root.</summary>
    public static string DatabaseFile => Path.Combine(Root, "MobileShop.db");

    /// <summary>Directory where application logs should be written, at the solution root.</summary>
    public static string LogDirectory => Path.Combine(Root, "MobileShop.Log");

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, "src")))
        {
            directory = directory.Parent;
        }
        return directory?.FullName ?? Directory.GetCurrentDirectory();
    }
}
