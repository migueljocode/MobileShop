using System.Runtime.CompilerServices;
using MobileShop.Services.PDF.Configuration;

namespace MobileShop.Tests;

internal static class ModuleInitializer
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        QuestPdfSetup.UseCommunityLicense();
    }
}
