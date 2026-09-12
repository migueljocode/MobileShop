namespace MobileShop.Tests.Services.DataServices.Dal;

public class PhoneDataServiceTests : RepoTestBase
{
    private readonly PhoneDataService _service;

    public PhoneDataServiceTests()
    {
        _service = new PhoneDataService(new PhoneRepo(Context), NullLogger<PhoneDataService>.Instance);
    }

    [Fact]
    public async Task Phone_service_supports_imei_profile_guarantee_and_secondhand_lookups()
    {
        var sellerPerson = new Person { FirstName = "Seller", LastName = "Phone", PhoneNumber = "09120000009" };
        Context.People.Add(sellerPerson);
        Context.SaveChanges();

        var seller = new Seller { PersonId = sellerPerson.Id, EntityType = SellerEntityType.Real };
        Context.Sellers.Add(seller);
        Context.SaveChanges();

        var customerPerson = new Person { FirstName = "Customer", LastName = "Phone", PhoneNumber = "09120000010" };
        Context.People.Add(customerPerson);
        Context.SaveChanges();

        var customer = new Customer { PersonId = customerPerson.Id, NationalId = "1000000005" };
        Context.Customers.Add(customer);
        Context.SaveChanges();

        var product = new Product { Manufacturer = "Nokia", Model = "Brick", Price = 50m };
        Context.Products.Add(product);
        Context.SaveChanges();

        var phone = new Phone { ProductId = product.Id, ProductNavigation = product, IMEI1 = "123456789012345", IMEI2 = "123456789012346", OwnershipTransferred = false };
        Context.Phones.Add(phone);
        Context.SaveChanges();

        var guarantee = new Guarantee { ProductId = product.Id, ProductNavigation = product, Corporation = "PhoneCare", StartDate = DateTime.UtcNow, ExpirationDate = DateTime.UtcNow.AddYears(1) };
        Context.Guarantees.Add(guarantee);

        var secondHand = new SecondHand { ProductId = product.Id, ProductNavigation = product, TestPeriodDays = 4, UsedDurationDays = 1 };
        Context.SecondHands.Add(secondHand);

        Context.Transactions.Add(new Transaction { ProductId = product.Id, ProductNavigation = product, SellerId = seller.Id, SellerNavigation = seller, CustomerId = customer.Id, CustomerNavigation = customer, FinishedPrice = 50m, Date = DateTime.UtcNow, Direction = TransactionDirection.Sell });
        Context.SaveChanges();

        Assert.True(_service.ImeiExists("123456789012345"));
        Assert.True(await _service.ImeiExistsAsync("123456789012345"));

        var owner = _service.GetOwner(phone.Id);
        Assert.NotNull(owner);
        Assert.Equal(customer.Id, owner!.Id);

        var ownerAsync = await _service.GetOwnerAsync(phone.Id);
        Assert.NotNull(ownerAsync);
        Assert.Equal(customer.Id, ownerAsync!.Id);

        var guaranteeOut = _service.GetGuarantee(phone.Id);
        Assert.NotNull(guaranteeOut);
        Assert.Equal("PhoneCare", guaranteeOut!.Corporation);

        var guaranteeOutAsync = await _service.GetGuaranteeAsync(phone.Id);
        Assert.NotNull(guaranteeOutAsync);
        Assert.Equal("PhoneCare", guaranteeOutAsync!.Corporation);

        var secondHandOut = _service.GetSecondHandInfo(phone.Id);
        Assert.NotNull(secondHandOut);
        Assert.Equal(4, secondHandOut!.TestPeriodDays);

        var secondHandOutAsync = await _service.GetSecondHandInfoAsync(phone.Id);
        Assert.NotNull(secondHandOutAsync);
        Assert.Equal(4, secondHandOutAsync!.TestPeriodDays);

        Assert.True(_service.Quantity() >= 0);
        Assert.True(_service.SecondHandQuantity() >= 1);
        Assert.True(_service.AvailableSecondHandQuantity() >= 0);

        var secondHandProducts = _service.GetSecondHand().ToList();
        Assert.Contains(secondHandProducts, p => p.Id == phone.Id);

        var availableSecondHand = _service.GetAvailableSecondHand().ToList();
        Assert.DoesNotContain(availableSecondHand, p => p.Id == phone.Id);

        var asyncSecondHand = await _service.GetSecondHandAsync();
        var asyncAvailable = await _service.GetAvailableSecondHandAsync();
        Assert.Contains(asyncSecondHand, p => p.Id == phone.Id);
        Assert.DoesNotContain(asyncAvailable, p => p.Id == phone.Id);
    }
}
