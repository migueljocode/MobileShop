namespace MobileShop.Tests.Models.ViewModels;

/// <summary>
/// Profit percent has no upper limit on any create/edit input: a percent derived from a large profit
/// amount must be accepted, while negative values stay invalid.
/// </summary>
public class ProfitPercentRangeTests
{
    public static TheoryData<Type> InputModelTypes => new()
    {
        typeof(CreatePhoneInputModel),
        typeof(CreateLaptopInputModel),
        typeof(CreateSmartWatchInputModel),
        typeof(CreateTabletInputModel),
        typeof(CreateAppleIdInputModel),
        typeof(EditProductInputModel),
    };

    [Theory]
    [MemberData(nameof(InputModelTypes))]
    public void ProfitPercent_accepts_values_above_100(Type modelType)
        => Assert.Empty(ValidateProfitPercent(modelType, 250.5m));

    [Theory]
    [MemberData(nameof(InputModelTypes))]
    public void ProfitPercent_rejects_negative_values(Type modelType)
        => Assert.Single(ValidateProfitPercent(modelType, -1m));

    private static List<ValidationResult> ValidateProfitPercent(Type modelType, decimal percent)
    {
        var instance = Activator.CreateInstance(modelType)!;
        var results = new List<ValidationResult>();
        Validator.TryValidateProperty(
            percent,
            new ValidationContext(instance) { MemberName = "ProfitPercent" },
            results);
        return results;
    }
}
