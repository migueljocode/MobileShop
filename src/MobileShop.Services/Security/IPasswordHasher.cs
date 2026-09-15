namespace MobileShop.Services.Security;

// should be used in DataServices.Interfaces.ISerDataService Implementation
/// <summary>
/// Defines the public contract for IPasswordHasher.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string hash, string password);
}
