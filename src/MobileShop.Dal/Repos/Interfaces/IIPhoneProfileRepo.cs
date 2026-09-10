namespace MobileShop.Dal.Repos.Interfaces;

// Named IPhoneProfileRepo, not IPhoneRepo - "IPhoneRepo" is already taken by the Phone entity's
// own repo interface, and once Repos (this class's namespace) and Repos.Interfaces are both in
// scope somewhere (e.g. DI registration later), an "IPhoneRepo" here would collide with it.
// "Profile" matches the existing Phone.IPhoneProfile navigation property name.
/// <summary>Repository for <see cref="IPhone"/> entities (iPhone-specific details attached to a <see cref="Phone"/>).</summary>
public interface IIPhoneProfileRepo : IBaseRepo<IPhone>
{
    
}
