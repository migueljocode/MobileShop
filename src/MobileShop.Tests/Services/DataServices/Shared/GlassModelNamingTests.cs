using MobileShop.Services.DataServices.Shared;

namespace MobileShop.Tests.Services.DataServices.Shared;

public class GlassModelNamingTests
{
    [Fact]
    public void Single_phone_keeps_the_original_name()
    {
        Assert.Equal("iPhone 16 Glass", GlassModelNaming.Build(new[] { "iPhone 16" }));
    }

    [Fact]
    public void Several_phones_are_sorted_and_joined()
    {
        Assert.Equal("Galaxy S24 / iPhone 16 Glass", GlassModelNaming.Build(new[] { "iPhone 16", "Galaxy S24" }));
    }

    [Fact]
    public void Order_of_the_phones_does_not_change_the_name()
    {
        var forward = GlassModelNaming.Build(new[] { "Galaxy S24", "iPhone 16", "Pixel 9" });
        var backward = GlassModelNaming.Build(new[] { "Pixel 9", "iPhone 16", "Galaxy S24" });

        Assert.Equal(forward, backward);
    }

    [Fact]
    public void Duplicate_names_ignoring_case_and_spaces_collapse_into_one()
    {
        Assert.Equal("iPhone 16 Glass", GlassModelNaming.Build(new[] { "iPhone 16", "iphone 16", " iPhone 16 " }));
    }

    [Fact]
    public void Very_long_single_name_is_shortened_to_the_model_name_limit()
    {
        var name = GlassModelNaming.Build(new[] { new string('A', 150) });

        Assert.Equal(GlassModelNaming.MaxNameLength, name.Length);
        Assert.EndsWith(" Glass", name);
    }

    [Fact]
    public void Long_list_keeps_as_many_names_as_fit_and_counts_the_rest()
    {
        var phones = Enumerable.Range(1, 10).Select(i => $"Phone {i:00} Model Long Name").ToArray();

        var name = GlassModelNaming.Build(phones);

        Assert.True(name.Length <= GlassModelNaming.MaxNameLength);
        Assert.Contains(" +7 more Glass", name);
        Assert.EndsWith(" Glass", name);
    }

    [Fact]
    public void No_phone_names_is_rejected()
    {
        Assert.Throws<ArgumentException>(() => GlassModelNaming.Build(Array.Empty<string>()));
    }
}
