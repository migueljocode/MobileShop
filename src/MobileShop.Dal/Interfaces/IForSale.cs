namespace MobileShop.Dal.Interfaces;

public interface IForSale<T> where T : BaseEntity // Product should not be implement IForSale but derived types in future should be like [Case, Charger etc.]
{
    bool IsSold(int id);
    Task<bool> IsSoldAsync(int id);

    /// <summary>
    /// 
    /// </summary>
    /// <returns>number of available count</returns>
    int Quantity();
    Task<int> QuantityAsync();

    /// <summary>
    /// see if product have guarantee
    /// </summary>
    /// <returns>The Guarantee object if any and null of none</returns>
    Guarantee? GetGuarantee(int id);
    Task<Guarantee?> GetGuaranteeAsync(int id);

    /// <summary>
    /// find owner of product if any
    /// </summary>
    /// <param name="id">id of product</param>
    /// <returns>an instance of its owner and if none returns nullF</returns>
    Customer? GetOwner(int id);
    Task<Customer?> GetOwnerAsync(int id);

    bool IsSecondHand(int id);
    Task<bool> IsSecondHandAsync(int id);
}