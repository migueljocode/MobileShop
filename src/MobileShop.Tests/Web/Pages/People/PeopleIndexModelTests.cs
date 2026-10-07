using MobileShop.Web.Pages.People;
using Moq;

namespace MobileShop.Tests.Web.Pages.People;

public class PeopleIndexModelTests
{
    private static Mock<IPeopleDataService> CreateService()
    {
        var service = new Mock<IPeopleDataService>();
        service.Setup(data => data.GetCustomerRowsAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(Array.Empty<CustomerListItemViewModel>());
        service.Setup(data => data.GetSellerRowsAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(Array.Empty<SellerListItemViewModel>());
        return service;
    }

    [Fact]
    public async Task OnGetAsync_AllShowsCustomersAndSellersWithoutAddActions()
    {
        var service = CreateService();
        service.Setup(data => data.GetCustomerRowsAsync("Name", true))
            .ReturnsAsync([new CustomerListItemViewModel(1, "A Customer", "111", "123", 2)]);
        service.Setup(data => data.GetSellerRowsAsync("Name", true))
            .ReturnsAsync([new SellerListItemViewModel(2, "B Seller", "222", "Real", 3)]);
        var model = new PeopleIndexModel(service.Object);

        await model.OnGetAsync();

        Assert.Equal("all", model.Type);
        Assert.Equal(["A Customer", "B Seller"], model.People.Select(person => person.Name));
        Assert.Equal("123", model.People.Single(person => person.Role == "Customer").NationalId);
        Assert.Null(model.People.Single(person => person.Role == "Seller").NationalId);
        Assert.False(model.ShowAddCustomer);
        Assert.False(model.ShowAddSeller);
    }

    [Theory]
    [InlineData("customers", true, false)]
    [InlineData("sellers", false, true)]
    public async Task OnGetAsync_TypeSelectsTheMatchingAddAction(string type, bool addCustomer, bool addSeller)
    {
        var service = CreateService();
        var model = new PeopleIndexModel(service.Object);

        await model.OnGetAsync(type);

        Assert.Equal(type, model.Type);
        Assert.Equal(addCustomer, model.ShowAddCustomer);
        Assert.Equal(addSeller, model.ShowAddSeller);
        service.Verify(data => data.GetCustomerRowsAsync(It.IsAny<string>(), It.IsAny<bool>()),
            type == "customers" ? Times.Once() : Times.Never());
        service.Verify(data => data.GetSellerRowsAsync(It.IsAny<string>(), It.IsAny<bool>()),
            type == "sellers" ? Times.Once() : Times.Never());
    }
}
