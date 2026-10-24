namespace MobileShop.Tests;

internal static class ModuleInitializer
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        QuestPdfSetup.UseCommunityLicense();
        QuestPdfSetup.RegisterFonts();
    }
}
