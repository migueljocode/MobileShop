namespace MobileShop.Dal.Repos.Interfaces;

/// <summary>Repository for <see cref="Employee"/> entities.</summary>
public interface IEmployeeRepo : IBaseRepo<Employee>
{
    /// <summary>Gets all active employees ordered by name.</summary>
    IReadOnlyList<Employee> FindAllActive();

    /// <summary>Gets all active employees asynchronously ordered by name.</summary>
    Task<IReadOnlyList<Employee>> FindAllActiveAsync();
}