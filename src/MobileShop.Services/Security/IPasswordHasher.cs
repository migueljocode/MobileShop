namespace MobileShop.Services.Security;

// should be used in DataServices.Interfaces.ISerDataService Implementation
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
