namespace MobileShop.Services.DataServices.Interfaces;

public interface IPhoneDataService : IDataService<Phone>
{
    bool ImeiExists(string imei1);
    Task<bool> ImeiExistsAsync(string imei1);

    // null ⇒ not sold
    // TODO: eager-load PersonNavigation when UI needs full customer details
    Customer? GetOwner(int id);
    Customer? GetOwner(Expression<Func<Phone, bool>> predicate);
    Task<Customer?> GetOwnerAsync(int id);
    Task<Customer?> GetOwnerAsync(Expression<Func<Phone, bool>> predicate);

    // TODO: eager-load Product when needed
    Guarantee? GetGuarantee(int id);
    Guarantee? GetGuarantee(Expression<Func<Phone, bool>> predicate);
    Task<Guarantee?> GetGuaranteeAsync(int id);
    Task<Guarantee?> GetGuaranteeAsync(Expression<Func<Phone, bool>> predicate);

    // null ⇒ not second-hand
    // TODO: eager-load ProductNavigation when UI needs product context
    SecondHand? GetSecondHandInfo(int id);
    SecondHand? GetSecondHandInfo(Expression<Func<Phone, bool>> predicate);
    Task<SecondHand?> GetSecondHandInfoAsync(int id);
    Task<SecondHand?> GetSecondHandInfoAsync(Expression<Func<Phone, bool>> predicate);

    int Quantity();
    int SecondHandQuantity();
    int AvailableSecondHandQuantity();
    Task<int> QuantityAsync();
    Task<int> SecondHandQuantityAsync();
    Task<int> AvailableSecondHandQuantityAsync();

    // TODO: explicit Include paths when list pages need Product / Guarantee / SecondHand
    IEnumerable<Phone> GetSecondHand();
    IEnumerable<Phone> GetAvailableSecondHand();
    Task<IEnumerable<Phone>> GetSecondHandAsync();
    Task<IEnumerable<Phone>> GetAvailableSecondHandAsync();
}