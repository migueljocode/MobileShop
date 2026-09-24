namespace MobileShop.Services.DataServices.Dal;

public class EmployeeDataService(
    IEmployeeRepo employeeRepo,
    ILogger<EmployeeDataService> logger)
    : DataServiceBase<EmployeeDataService, Employee>(employeeRepo, logger), IEmployeeDataService
{
    private readonly IEmployeeRepo _employeeRepo = employeeRepo;

    /// <inheritdoc />
    public IReadOnlyList<Employee> GetActiveEmployees()
        => _employeeRepo.FindAllActive();

    /// <inheritdoc />
    public async Task<IReadOnlyList<Employee>> GetActiveEmployeesAsync()
        => await _employeeRepo.FindAllActiveAsync();
}