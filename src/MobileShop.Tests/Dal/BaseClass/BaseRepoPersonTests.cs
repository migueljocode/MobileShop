namespace MobileShop.Tests.Dal.BaseClass;

/// <summary>
/// Concrete suite that keeps the generic <see cref="BaseRepo{T}"/> coverage running after the
/// specialized per-entity repositories were deleted in Stage H.
/// </summary>
/// <remarks>
/// <see cref="BaseRepoTests{TEntity,TRepo}"/> is abstract, so its tests only execute through a
/// concrete subclass. Every former subclass was tied to a now-deleted per-entity repo; this one
/// exercises the surviving generic <see cref="IBaseRepo{T}"/> contract directly.
/// </remarks>
public class BaseRepoPersonTests : BaseRepoTests<Person, BaseRepo<Person>>
{
    protected override BaseRepo<Person> CreateRepo() => new(Context);

    protected override Person CreateValidEntity() => new()
    {
        FirstName = "Test",
        LastName = "Person",
        PhoneNumber = "09120000000"
    };
}
