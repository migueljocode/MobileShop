using Microsoft.AspNetCore.Mvc;
using MobileShop.Web.Pages.Transactions;

namespace MobileShop.Tests.Web.Pages.Transactions;

public class PersonCreateHandlerTests
{
    [Fact]
    public async Task Sell_OnPostCreateCustomerAsync_returns_new_customer_option()
    {
        var people = new StubPeopleDataService
        {
            CreateCustomerResult = new ServiceResult(true, null, EntityId: 24)
        };
        var model = new SellModel(null!, people);
        model.ModelState.AddModelError("Input.ProductId", "The product is required.");

        var result = await model.OnPostCreateCustomerAsync(new CreateCustomerInputModel
        {
            FirstName = "Sara",
            LastName = " Ahmadi ",
            PhoneNumber = "09120000002",
            NationalId = "1234567890"
        });

        var json = Assert.IsType<JsonResult>(result);
        var payload = Assert.IsType<DropdownCreateResult>(json.Value);
        Assert.True(payload.Succeeded);
        Assert.Equal(24, payload.Option!.Id);
        Assert.Equal("Sara Ahmadi", payload.Option.Name);
        Assert.Equal(200, payload.StatusCode);
    }

    private sealed class StubPeopleDataService : IPeopleDataService
    {
        public ServiceResult CreateCustomerResult { get; init; } = new(true, null, EntityId: 1);

        public Task<IReadOnlyList<CustomerListItemViewModel>> GetCustomerRowsAsync(string sortBy, bool ascending) => throw new NotImplementedException();
        public Task<IReadOnlyList<SellerListItemViewModel>> GetSellerRowsAsync(string sortBy, bool ascending) => throw new NotImplementedException();
        public Task<CustomerDetailsViewModel?> GetCustomerDetailsAsync(int id) => throw new NotImplementedException();
        public Task<SellerDetailsViewModel?> GetSellerDetailsAsync(int id) => throw new NotImplementedException();
        public Task<IReadOnlyList<PartyOptionViewModel>> SearchCustomersAsync(string? q, int take = 25) => throw new NotImplementedException();
        public Task<IReadOnlyList<PartyOptionViewModel>> SearchSellersAsync(string? q, int take = 25) => throw new NotImplementedException();
        public Task<ServiceResult> CreateCustomerAsync(CreateCustomerInputModel input) => Task.FromResult(CreateCustomerResult);
        public Task<ServiceResult> CreateSellerAsync(CreateSellerInputModel input) => throw new NotImplementedException();
    }
}
