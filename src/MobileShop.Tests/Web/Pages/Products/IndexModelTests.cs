using MobileShop.Web.Pages.Products;
using Moq;

namespace MobileShop.Tests.Web.Pages.Products;

public class IndexModelTests
{
    private static (Mock<IProductsDataService> service, IndexModel model) CreateModel()
    {
        var service = new Mock<IProductsDataService>();
        service.Setup(s => s.GetInventoryRowsAsync(
                It.IsAny<string?>(),
                It.IsAny<int?>(),
                It.IsAny<string?>()))
            .ReturnsAsync(Array.Empty<ProductListItemViewModel>());
        service.Setup(s => s.GetManufacturersAsync())
            .ReturnsAsync(Array.Empty<DropdownOptionViewModel>());
        service.Setup(s => s.GetModelsAsync(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(Array.Empty<DropdownOptionViewModel>());

        return (service, new IndexModel(service.Object));
    }

    [Fact]
    public async Task OnGetAsync_DefaultsAvailabilityToAllAndPassesNull()
    {
        var (service, model) = CreateModel();

        await model.OnGetAsync();

        Assert.Equal("all", model.Availability);
        service.Verify(s => s.GetInventoryRowsAsync("all", null, null), Times.Once);
    }

    [Theory]
    [InlineData("Sold")]
    [InlineData(" sold ")]
    public async Task OnGetAsync_NormalizesSoldAvailability(string availability)
    {
        var (service, model) = CreateModel();

        await model.OnGetAsync(availability: availability);

        Assert.Equal("sold", model.Availability);
        service.Verify(s => s.GetInventoryRowsAsync("all", null, "sold"), Times.Once);
    }

    [Fact]
    public async Task OnGetAsync_NormalizesAvailableAvailability()
    {
        var (service, model) = CreateModel();

        await model.OnGetAsync(availability: "available");

        Assert.Equal("available", model.Availability);
        service.Verify(s => s.GetInventoryRowsAsync("all", null, "available"), Times.Once);
    }

    [Fact]
    public async Task OnGetAsync_UnknownAvailabilityDefaultsToAllAndPassesNull()
    {
        var (service, model) = CreateModel();

        await model.OnGetAsync(availability: "bogus");

        Assert.Equal("all", model.Availability);
        service.Verify(s => s.GetInventoryRowsAsync("all", null, null), Times.Once);
    }

    [Fact]
    public async Task OnGetAsync_PhonePartNumberAndAvailabilityArePassedTogether()
    {
        var (service, model) = CreateModel();
        service.Setup(s => s.GetInventoryPartNumbersAsync())
            .ReturnsAsync(new[] { new DropdownOptionViewModel(42, "A-42") });

        await model.OnGetAsync(type: "phone", partNumberId: 42, availability: " sold ");

        Assert.Equal("phone", model.Type);
        Assert.Equal(42, model.PartNumberId);
        Assert.Equal("sold", model.Availability);
        service.Verify(s => s.GetInventoryRowsAsync("phone", 42, "sold"), Times.Once);
    }

    [Fact]
    public async Task AppleIdNameColumnIsCustomerAndCanBeSortedInEitherDirection()
    {
        var (service, model) = CreateModel();
        service.Setup(s => s.GetInventoryRowsAsync("appleid", null, null))
            .ReturnsAsync(new[]
            {
                new ProductListItemViewModel(1, 1, "Apple ID", "Zoe Customer", "z@example.com", null, true, false),
                new ProductListItemViewModel(2, 2, "Apple ID", "Amy Customer", "a@example.com", null, true, false),
            });

        await model.OnGetAsync(type: "appleid", sortBy: "name");

        Assert.Equal("Customer", model.NameColumnLabel);
        Assert.Equal(["Amy Customer", "Zoe Customer"], model.Products.Select(product => product.Name));

        await model.OnGetAsync(type: "appleid", sortBy: "name", sortDirection: "desc");

        Assert.Equal(["Zoe Customer", "Amy Customer"], model.Products.Select(product => product.Name));
    }
}
