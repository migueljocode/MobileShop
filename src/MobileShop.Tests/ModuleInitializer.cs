using System.Runtime.CompilerServices;

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
