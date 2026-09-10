using MobileShop.Dal.Interfaces;

namespace MobileShop.Dal.Repos.Interfaces;

/// <summary>Repository for <see cref="AppleId"/> entities.</summary>
public interface IAppleIdRepo : IBaseRepo<AppleId>, IForSale<AppleId>
{
    AppleId? Find(string email);
    Task<AppleId?> FindAsync(string email);
}