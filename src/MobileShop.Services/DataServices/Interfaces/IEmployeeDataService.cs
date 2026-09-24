namespace MobileShop.Services.DataServices.Interfaces;

/// <summary>
/// Defines the public contract for IEmployeeDataService.
/// </summary>
public interface IEmployeeDataService : IDataService<Employee>
{
    /// <summary>Gets all active employees.</summary>
    IReadOnlyList<Employee> GetActiveEmployees();
    
    /// <summary>Gets all active employees asynchronously.</summary>
    Task<IReadOnlyList<Employee>> GetActiveEmployeesAsync();
}