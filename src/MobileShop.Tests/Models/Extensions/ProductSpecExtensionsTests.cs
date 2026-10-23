namespace MobileShop.Tests.Models.Extensions;

public class ProductSpecExtensionsTests
{
    private static Product MakePhone(string barcode, int colorId, int storageGb = 256) => new()
    {
        Barcode = barcode,
        ModelId = 1,
        ColorId = colorId,
        ColorNavigation = new Color { Name = colorId == 1 ? "Blue" : "Green" },
        DeviceSpecProfile = new DeviceSpec
        {
            Ram = 8,
            StorageCapacityId = storageGb,
            ChargeSpeed = 25,
            StorageCapacityNavigation = new StorageCapacity { Gb = storageGb },
        },
        PhoneProfile = new Phone { IMEI1 = "123456789012345" },
    };

    [Fact]
    public void Same_specs_with_different_barcode_and_imei_share_a_key()
    {
        var first = MakePhone("A-1", colorId: 1);
        var second = MakePhone("A-2", colorId: 1);
        second.PhoneProfile!.IMEI1 = "999999999999999";

        Assert.Equal(first.ToSpecKey(), second.ToSpecKey());
    }

    [Fact]
    public void Different_colour_gives_a_different_key()
    {
        Assert.NotEqual(MakePhone("A-1", colorId: 1).ToSpecKey(), MakePhone("A-2", colorId: 2).ToSpecKey());
    }

    [Fact]
    public void Different_built_in_storage_gives_a_different_key()
    {
        Assert.NotEqual(MakePhone("A-1", 1, 128).ToSpecKey(), MakePhone("A-2", 1, 256).ToSpecKey());
    }

    [Fact]
    public void Different_cable_length_gives_a_different_key()
    {
        var shortCable = new Product { ModelId = 2, CableProfile = new Cable { Length = 1.0m } };
        var longCable = new Product { ModelId = 2, CableProfile = new Cable { Length = 2.0m } };

        Assert.NotEqual(shortCable.ToSpecKey(), longCable.ToSpecKey());
    }

    [Fact]
    public void Glass_fitting_a_different_model_gives_a_different_key()
    {
        var forOne = new Product { ModelId = 5, GlassProfile = new Glass { ModelFits = [new GlassModelFit { ModelId = 10 }] } };
        var forTwo = new Product { ModelId = 5, GlassProfile = new Glass { ModelFits = [new GlassModelFit { ModelId = 11 }] } };

        Assert.NotEqual(forOne.ToSpecKey(), forTwo.ToSpecKey());
    }

    [Fact]
    public void Glass_fit_order_does_not_change_the_key()
    {
        var ordered = new Product
        {
            ModelId = 5,
            GlassProfile = new Glass { ModelFits = [new GlassModelFit { ModelId = 10 }, new GlassModelFit { ModelId = 11 }] },
        };
        var reversed = new Product
        {
            ModelId = 5,
            GlassProfile = new Glass { ModelFits = [new GlassModelFit { ModelId = 11 }, new GlassModelFit { ModelId = 10 }] },
        };

        Assert.Equal(ordered.ToSpecKey(), reversed.ToSpecKey());
    }

    [Fact]
    public void Second_hand_and_new_products_have_different_keys()
    {
        var newOne = new Product { ModelId = 3 };
        var used = new Product { ModelId = 3, SecondHandProfile = new SecondHand() };

        Assert.NotEqual(newOne.ToSpecKey(), used.ToSpecKey());
    }

    [Fact]
    public void Product_without_profiles_keys_on_model_and_colour_only()
    {
        var plain = new Product { ModelId = 4, ColorId = null };

        Assert.False(string.IsNullOrEmpty(plain.ToSpecKey()));
        Assert.Equal(new Product { ModelId = 4 }.ToSpecKey(), plain.ToSpecKey());
    }

    [Fact]
    public void Label_names_colour_and_built_in_storage()
    {
        Assert.Equal("Blue · 256 GB", MakePhone("A-1", colorId: 1).ToSpecLabel());
    }

    [Fact]
    public void Label_is_empty_when_colour_and_storage_are_unknown()
    {
        Assert.Equal(string.Empty, new Product { ModelId = 9 }.ToSpecLabel());
    }
}
