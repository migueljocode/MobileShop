namespace MobileShop.Tests.Services.DataServices.Dal;

public class UserDataServiceTests : RepoTestBase
{
    private readonly UserDataService _service;

    public UserDataServiceTests()
    {
        _service = new UserDataService(new UserRepo(Context), new PasswordHasher(), NullLogger<UserDataService>.Instance);
    }

    [Fact]
    public void Create_Validate_ChangePassword_flow_works()
    {
        var person = new Person { FirstName = "Dana", LastName = "Tester", PhoneNumber = "09120000000" };
        Context.People.Add(person);
        Context.SaveChanges();

        var created = _service.Create(new User { PersonId = person.Id, Username = "dana", PasswordHash = string.Empty }, "Secret123!");
        Assert.True(created);

        var found = _service.FindByUsername("dana");
        Assert.NotNull(found);
        Assert.True(_service.ValidateCredentials("dana", "Secret123!"));
        Assert.False(_service.ValidateCredentials("dana", "WrongPassword!"));

        var changed = _service.ChangePassword("dana", "NewSecret123!");
        Assert.True(changed);

        Assert.True(_service.ValidateCredentials("dana", "NewSecret123!"));
        Assert.False(_service.ValidateCredentials("dana", "Secret123!"));
    }

    [Fact]
    public async Task CreateAsync_ValidateCredentialsAsync_ChangePasswordAsync_flow_works()
    {
        var person = new Person { FirstName = "Dana", LastName = "Async", PhoneNumber = "09120000011" };
        Context.People.Add(person);
        Context.SaveChanges();

        var created = await _service.CreateAsync(new User { PersonId = person.Id, Username = "asyncdana", PasswordHash = string.Empty }, "Secret123!");
        Assert.True(created);

        var found = await _service.FindByUsernameAsync("asyncdana");
        Assert.NotNull(found);

        Assert.True(await _service.ValidateCredentialsAsync("asyncdana", "Secret123!"));
        Assert.False(await _service.ValidateCredentialsAsync("asyncdana", "WrongPassword!"));

        var changed = await _service.ChangePasswordAsync("asyncdana", "NewSecret456!");
        Assert.True(changed);

        Assert.True(await _service.ValidateCredentialsAsync("asyncdana", "NewSecret456!"));
        Assert.False(await _service.ValidateCredentialsAsync("asyncdana", "Secret123!"));
    }
}
