namespace MobileShop.Services.PDF.Configuration;

/// <summary>
/// Centralizes the QuestPDF startup configuration required before the first render.
/// </summary>
public static class QuestPdfSetup
{
    /// <summary>
    /// Sets the community licence once before the first PDF render.
    /// </summary>
    public static void UseCommunityLicense()
        => QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

    /// <summary>
    /// Registers the bundled Vazirmatn TTF font files with QuestPDF's
    /// <see cref="FontManager"/> so that the <c>Vazirmatn</c> family name used by
    /// <see cref="QuestPdfGenerator.GeneratePersian"/> resolves at render time.
    /// </summary>
    /// <remarks>
    /// Fonts are registered from the application base directory (the copied output)
    /// so that the registration path works for both the Web host and direct unit tests.
    /// </remarks>
    public static void RegisterFonts()
    {
        var fontDirectory = Path.Combine(AppContext.BaseDirectory, "PDF", "Fonts");
        if (Directory.Exists(fontDirectory))
            QuestPDF.Drawing.FontManager.RegisterFontsFromDirectory(fontDirectory);
    }
}
