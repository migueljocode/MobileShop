namespace MobileShop.Tests.Dal.BaseClass;

internal static class TestDataHelpers
{
    // a syntactically valid 15-digit IMEI - the range guarantees exactly 15 digits every time,
    // and Random.Shared makes repeated calls within a test run unlikely to collide
    internal static string GenerateImei() => Random.Shared.NextInt64(100_000_000_000_000, 999_999_999_999_999).ToString();
}
