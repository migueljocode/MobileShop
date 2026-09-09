namespace MobileShop.Tests.Dal.Repos;

public class PersonRepoTests : BaseRepoTests<Person, IPersonRepo>
{
    protected override IPersonRepo CreateRepo() => new PersonRepo(Context);

    protected override Person CreateValidEntity() => new()
    {
        FirstName = "Test",
        LastName = "Person",
        PhoneNumber = "09120000000"
    };
}
