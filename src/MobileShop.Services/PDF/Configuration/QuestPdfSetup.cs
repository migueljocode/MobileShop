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
}
