namespace MobileShop.Dal.Repos.Interfaces;

/// <summary>Repository for <see cref="AppleId"/> entities. Also see <see cref="IForSale{T}"/>.</summary>
public interface IAppleIdRepo : IBaseRepo<AppleId>, IForSale<AppleId>
{
    /// <summary>Finds an Apple ID by its email, case-insensitively.</summary>
    /// <param name="email">The email to look up.</param>
    /// <returns>The matching Apple ID, or <see langword="null"/> if none is found.</returns>
    AppleId? Find(string email);

    /// <summary>Asynchronous version of <see cref="Find(string)"/>.</summary>
    /// <param name="email">The email to look up.</param>
    /// <returns>The matching Apple ID, or <see langword="null"/> if none is found.</returns>
    Task<AppleId?> FindAsync(string email);
}