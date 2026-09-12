namespace MobileShop.Services.DataServices.Interfaces;

public interface IAppleIdDataService : IDataService<AppleId>
{
    AppleId? FindByEmail(string email);
    Task<AppleId?> FindByEmailAsync(string email);

    // null ⇒ not sold
    // TODO: eager-load PersonNavigation when UI needs full customer details
    Customer? GetOwner(int id);
    Customer? GetOwner(Expression<Func<AppleId, bool>> predicate);
    Task<Customer?> GetOwnerAsync(int id);
    Task<Customer?> GetOwnerAsync(Expression<Func<AppleId, bool>> predicate);

    // TODO: eager-load Product when needed
    Guarantee? GetGuarantee(int id);
    Guarantee? GetGuarantee(Expression<Func<AppleId, bool>> predicate);
    Task<Guarantee?> GetGuaranteeAsync(int id);
    Task<Guarantee?> GetGuaranteeAsync(Expression<Func<AppleId, bool>> predicate);

    // null ⇒ not second-hand
    SecondHand? GetSecondHandInfo(int id);
    SecondHand? GetSecondHandInfo(Expression<Func<AppleId, bool>> predicate);
    Task<SecondHand?> GetSecondHandInfoAsync(int id);
    Task<SecondHand?> GetSecondHandInfoAsync(Expression<Func<AppleId, bool>> predicate);

    int Quantity();
    int SecondHandQuantity();
    int AvailableSecondHandQuantity();
    Task<int> QuantityAsync();
    Task<int> SecondHandQuantityAsync();
    Task<int> AvailableSecondHandQuantityAsync();

    // TODO: explicit Include paths when list pages need related data
    IEnumerable<AppleId> GetSecondHand();
    IEnumerable<AppleId> GetAvailableSecondHand();
    Task<IEnumerable<AppleId>> GetSecondHandAsync();
    Task<IEnumerable<AppleId>> GetAvailableSecondHandAsync();
}