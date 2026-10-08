namespace MobileShop.Dal.EfStructures;

/// <summary>
/// Resolves well-known paths relative to the configured data directory or solution root.
/// </summary>
public static class SolutionPaths
{
    /// <summary>The solution root directory.</summary>
    public static string Root { get; } = ResolveRoot();

    /// <summary>Full path to the SQLite database file, at the solution root.</summary>
    public static string DatabaseFile => Path.Combine(Root, "MobileShop.db");

    /// <summary>Directory where application logs should be written, at the solution root.</summary>
    public static string LogDirectory => Path.Combine(Root, "MobileShop.Log");

    private static string ResolveRoot()
    {
        var configuredDirectory = Environment.GetEnvironmentVariable("MOBILESHOP_DATA_DIRECTORY");
        return string.IsNullOrWhiteSpace(configuredDirectory)
            ? FindRoot()
            : Path.GetFullPath(configuredDirectory);
    }

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        if (File.Exists(Path.Combine(directory.FullName, "MobileShop.Web.staticwebassets.endpoints.json")))
        {
            return directory.FullName;
        }

        while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, "src")))
        {
            directory = directory.Parent;
        }
        return directory?.FullName ?? Directory.GetCurrentDirectory();
    }
}
