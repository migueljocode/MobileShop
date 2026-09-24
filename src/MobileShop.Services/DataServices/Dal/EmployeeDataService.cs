namespace MobileShop.Services.DataServices.Dal;

public class EmployeeDataService(
    IEmployeeRepo employeeRepo,
    ILogger<EmployeeDataService> logger)
    : DataServiceBase<EmployeeDataService, Employee>(employeeRepo, logger), IEmployeeDataService
{
    private readonly IEmployeeRepo _employeeRepo = employeeRepo;

    /// <inheritdoc />
    public IReadOnlyList<Employee> GetActiveEmployees()
        => _employeeRepo
            .FindAll(e => e.IsActive)
            .OrderBy(e => e.PersonNavigation.FirstName)
            .ThenBy(e => e.PersonNavigation.LastName)
            .ToList();

    /// <inheritdoc />
    public async Task<IReadOnlyList<Employee>> GetActiveEmployeesAsync()
    {
        var employees = await _employeeRepo.FindAllAsync(e => e.IsActive);
        return employees
            .OrderBy(e => e.PersonNavigation.FirstName)
            .ThenBy(e => e.PersonNavigation.LastName)
            .ToList();
    }
}