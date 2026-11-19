using System.Reflection;

namespace MobileShop.Web.Extensions;

/// <summary>
/// Formats the application version string reported by <c>--version</c>.
/// </summary>
public static class VersionInfo
{
    /// <summary>
    /// Returns the assembly's informational version, falling back to its assembly version or <c>0.0.0</c>.
    /// </summary>
    public static string Describe(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        var informationalVersion = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;

        if (!string.IsNullOrWhiteSpace(informationalVersion))
        {
            return informationalVersion;
        }

        return assembly.GetName().Version?.ToString() ?? "0.0.0";
    }
}
