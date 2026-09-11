namespace MobileShop.Dal.Interfaces;

/// <summary>
/// Shared read operations for entities that represent an individually-sold, serialized item
/// (one row = one physical unit, like <see cref="Phone"/> or <see cref="AppleId"/>) - as opposed to
/// a quantity-tracked good. <see cref="Product"/> itself should not implement this; it's meant for
/// derived/attached types (and future ones, e.g. a Case or Charger entity, should implement it too).
/// </summary>
/// <typeparam name="T">The entity type, which must derive from <see cref="BaseEntity"/>.</typeparam>
public interface IForSale<T> where T : BaseEntity
{
    /// <summary>Checks whether the item has been sold (has a Sell-direction transaction against its product).</summary>
    /// <param name="id">The Id of the item to check.</param>
    /// <returns><see langword="true"/> if it has been sold.</returns>
    bool IsSold(int id);

    /// <summary>Asynchronous version of <see cref="IsSold"/>.</summary>
    /// <param name="id">The Id of the item to check.</param>
    /// <returns><see langword="true"/> if it has been sold.</returns>
    Task<bool> IsSoldAsync(int id);

    /// <summary>Counts how many items of this type are currently in stock (not yet sold), across all records.</summary>
    /// <returns>The number of available items.</returns>
    int Quantity();

    /// <summary>Asynchronous version of <see cref="Quantity"/>.</summary>
    /// <returns>The number of available items.</returns>
    Task<int> QuantityAsync();

    /// <summary>Gets the guarantee attached to the item's product, if any.</summary>
    /// <param name="id">The Id of the item.</param>
    /// <returns>The <see cref="Guarantee"/>, or <see langword="null"/> if the product has none.</returns>
    Guarantee? GetGuarantee(int id);

    /// <summary>Asynchronous version of <see cref="GetGuarantee"/>.</summary>
    /// <param name="id">The Id of the item.</param>
    /// <returns>The <see cref="Guarantee"/>, or <see langword="null"/> if the product has none.</returns>
    Task<Guarantee?> GetGuaranteeAsync(int id);

    /// <summary>Finds the current owner of the item - the customer on its most recent Sell-direction transaction, if any.</summary>
    /// <param name="id">The Id of the item.</param>
    /// <returns>The owning <see cref="Customer"/>, or <see langword="null"/> if it hasn't been sold.</returns>
    Customer? GetOwner(int id);

    /// <summary>Asynchronous version of <see cref="GetOwner"/>.</summary>
    /// <param name="id">The Id of the item.</param>
    /// <returns>The owning <see cref="Customer"/>, or <see langword="null"/> if it hasn't been sold.</returns>
    Task<Customer?> GetOwnerAsync(int id);

    /// <summary>Checks whether the item's product has a <see cref="SecondHand"/> profile attached.</summary>
    /// <param name="id">The Id of the item.</param>
    /// <returns><see langword="true"/> if it's secondhand.</returns>
    bool IsSecondHand(int id);

    /// <summary>Asynchronous version of <see cref="IsSecondHand"/>.</summary>
    /// <param name="id">The Id of the item.</param>
    /// <returns><see langword="true"/> if it's secondhand.</returns>
    Task<bool> IsSecondHandAsync(int id);
}