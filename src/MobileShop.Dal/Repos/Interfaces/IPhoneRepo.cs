namespace MobileShop.Dal.Repos.Interfaces;

/// <summary>Repository for <see cref="Phone"/> entities. Also see <see cref="IForSale{T}"/>.</summary>
public interface IPhoneRepo : IBaseRepo<Phone>, IForSale<Phone>
{
    /// <summary>Checks whether a phone with the given <see cref="Phone.IMEI1"/> already exists.</summary>
    /// <param name="imei1">The 15-digit IMEI to check.</param>
    /// <returns><see langword="true"/> if a phone with that IMEI1 is already on record.</returns>
    bool ImeiExists(string imei1);

    /// <summary>Asynchronous version of <see cref="ImeiExists"/>.</summary>
    /// <param name="imei1">The 15-digit IMEI to check.</param>
    /// <returns><see langword="true"/> if a phone with that IMEI1 is already on record.</returns>
    Task<bool> ImeiExistsAsync(string imei1);

    // Interface
    SecondHand? GetSecondHandInfo(int id);
    Task<SecondHand?> GetSecondHandInfoAsync(int id);
}