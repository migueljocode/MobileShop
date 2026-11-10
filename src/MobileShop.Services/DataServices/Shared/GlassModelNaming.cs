namespace MobileShop.Services.DataServices.Shared;

/// <summary>
/// Builds the model name of a glass product from the phones it fits, so one glass that fits several
/// phones gets one stable name whatever order the phones were picked in.
/// </summary>
public static class GlassModelNaming
{
    /// <summary>The longest name a model may have (<see cref="Model.Name"/> is limited to 100 characters).</summary>
    public const int MaxNameLength = 100;

    private const string Suffix = " Glass";
    private const string Separator = " / ";

    /// <param name="compatibleModelNames">The display names of the phones the glass fits; at least one.</param>
    public static string Build(IReadOnlyCollection<string> compatibleModelNames)
    {
        var names = compatibleModelNames
            .Select(name => name.Trim())
            .Where(name => name.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (names.Count == 0)
            throw new ArgumentException("At least one compatible phone name is required.", nameof(compatibleModelNames));

        if (names.Count == 1)
            return names[0][..Math.Min(MaxNameLength - Suffix.Length, names[0].Length)] + Suffix;

        var full = string.Join(Separator, names) + Suffix;
        if (full.Length <= MaxNameLength)
            return full;

        // Keep as many leading names as fit beside a "+N more" tail.
        for (var kept = names.Count - 1; kept >= 1; kept--)
        {
            var candidate = string.Join(Separator, names.Take(kept)) + $" +{names.Count - kept} more" + Suffix;
            if (candidate.Length <= MaxNameLength)
                return candidate;
        }

        // Even one name does not fit beside the tail, so shorten the first name.
        var tail = $" +{names.Count - 1} more" + Suffix;
        var room = MaxNameLength - tail.Length;
        return names[0][..Math.Min(room, names[0].Length)] + tail;
    }
}
