namespace MobileShop.Tests.Services.DataServices.Dal;

public class AppleIdDataServiceTests : RepoTestBase
{
    private readonly AppleIdDataService _service;

    public AppleIdDataServiceTests()
    {
        _service = new AppleIdDataService(new AppleIdRepo(Context), NullLogger<AppleIdDataService>.Instance);
    }

    [Fact]
    public async Task AppleId_service_supports_find_owner_guarantee_and_secondhand_accessors()
    {
        var sellerPerson = new Person { FirstName = "Seller", LastName = "Apple", PhoneNumber = "09120000005" };
        Context.People.Add(sellerPerson);
        Context.SaveChanges();

        var seller = new Seller { PersonId = sellerPerson.Id, EntityType = SellerEntityType.Real };
        Context.Sellers.Add(seller);
        Context.SaveChanges();

        var customerPerson = new Person { FirstName = "Customer", LastName = "Apple", PhoneNumber = "09120000006" };
        Context.People.Add(customerPerson);
        Context.SaveChanges();

        var customer = new Customer { PersonId = customerPerson.Id, NationalId = "1000000003" };
        Context.Customers.Add(customer);
        Context.SaveChanges();

        var product = TestDataHelpers.CreateProduct(Context, 60m);

        var appleId = new AppleId { ProductId = product.Id, ProductNavigation = product, Email = "owner@example.com", Password = "secret", Notes = "owner note" };
        Context.AppleIds.Add(appleId);

        var guarantee = new Guarantee { ProductId = product.Id, ProductNavigation = product, Corporation = "AppleCare", StartDate = DateTime.UtcNow, ExpirationDate = DateTime.UtcNow.AddYears(1) };
        Context.Guarantees.Add(guarantee);

        var secondHand = new SecondHand { ProductId = product.Id, ProductNavigation = product, TestPeriodDays = 7, UsedDurationDays = 0 };
        Context.SecondHands.Add(secondHand);

        Context.Transactions.Add(new Transaction { ProductId = product.Id, ProductNavigation = product, SellerId = seller.Id, SellerNavigation = seller, CustomerId = customer.Id, CustomerNavigation = customer, FinishedPrice = 60m, Date = DateTime.UtcNow, Direction = TransactionDirection.Sell });
        Context.SaveChanges();

        var byEmail = _service.FindByEmail("OWNER@example.com");
        Assert.NotNull(byEmail);
        Assert.Equal(product.Id, byEmail!.ProductId);

        var byEmailAsync = await _service.FindByEmailAsync("OWNER@example.com");
        Assert.NotNull(byEmailAsync);

        var owner = _service.GetOwner(byEmail.Id);
        Assert.NotNull(owner);
        Assert.Equal(customer.Id, owner!.Id);

        var ownerAsync = await _service.GetOwnerAsync(byEmail.Id);
        Assert.NotNull(ownerAsync);
        Assert.Equal(customer.Id, ownerAsync!.Id);

        var guaranteeOut = _service.GetGuarantee(byEmail.Id);
        Assert.NotNull(guaranteeOut);
        Assert.Equal("AppleCare", guaranteeOut!.Corporation);

        var guaranteeOutAsync = await _service.GetGuaranteeAsync(byEmail.Id);
        Assert.NotNull(guaranteeOutAsync);

        var secondHandOut = _service.GetSecondHandInfo(byEmail.Id);
        Assert.NotNull(secondHandOut);
        Assert.Equal(7, secondHandOut!.TestPeriodDays);

        var secondHandOutAsync = await _service.GetSecondHandInfoAsync(byEmail.Id);
        Assert.NotNull(secondHandOutAsync);
        Assert.Equal(7, secondHandOutAsync!.TestPeriodDays);

        Assert.True(_service.Quantity() >= 0);
        Assert.True(_service.SecondHandQuantity() >= 1);
        Assert.True(_service.AvailableSecondHandQuantity() >= 0);

        var secondHandProducts = _service.GetSecondHand().ToList();
        Assert.Contains(secondHandProducts, p => p.Id == appleId.Id);

        var availableSecondHand = _service.GetAvailableSecondHand().ToList();
        Assert.DoesNotContain(availableSecondHand, p => p.Id == appleId.Id);

        var secondHandProductsAsync = await _service.GetSecondHandAsync();
        var availableAsync = await _service.GetAvailableSecondHandAsync();
        Assert.Contains(secondHandProductsAsync, p => p.Id == appleId.Id);
        Assert.DoesNotContain(availableAsync, p => p.Id == appleId.Id);
    }
}
