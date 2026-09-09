using MobileShop.Dal.Interfaces;

namespace MobileShop.Dal.Repos.Interfaces;

/// <summary>Repository for <see cref="AppleId"/> entities.</summary>
public interface IAppleIdRepo : IBaseRepo<AppleId>, IForSale<AppleId>
{
    AppleId? Find(string email);
    Task<AppleId?> FindAsync(string email);

    /// <summary>
    /// finds user purchased this AppleId
    /// </summary>
    /// <param name="id">the appleId's id</param>
    /// <returns>User if found and null if no user purchased this</returns>
    Customer? FindUser(int id);
    Task<Customer?> FindUserAsync(int id);
}