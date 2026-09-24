namespace MobileShop.Dal.Repos;

/// <inheritdoc cref="IEmployeeRepo" />
public class EmployeeRepo(AppDbContext context) : BaseRepo<Employee>(context), IEmployeeRepo
{
    public IReadOnlyList<Employee> FindAllActive()
        => Table
            .Where(e => e.IsActive)
            .Include(e => e.PersonNavigation)
            .OrderBy(e => e.PersonNavigation.FirstName)
            .ThenBy(e => e.PersonNavigation.LastName)
            .ToList();

    public async Task<IReadOnlyList<Employee>> FindAllActiveAsync()
    {
        var employees = await Table
            .Where(e => e.IsActive)
            .Include(e => e.PersonNavigation)
            .OrderBy(e => e.PersonNavigation.FirstName)
            .ThenBy(e => e.PersonNavigation.LastName)
            .ToListAsync();
        return employees;
    }
}