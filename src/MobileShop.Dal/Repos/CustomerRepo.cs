namespace MobileShop.Dal.Repos;

/// <inheritdoc cref="ICustomerRepo" />
public class CustomerRepo(AppDbContext context) : BaseRepo<Customer>(context), ICustomerRepo
{
    public override IEnumerable<Customer> FindAll(Expression<Func<Customer, bool>>? predicate = null)
    {
        IQueryable<Customer> query = Table.Include(c => c.PersonNavigation);
        return (predicate is null ? query : query.Where(predicate)).ToList();
    }

    public override async Task<IEnumerable<Customer>> FindAllAsync(Expression<Func<Customer, bool>>? predicate = null)
    {
        IQueryable<Customer> query = Table.Include(c => c.PersonNavigation);
        return await (predicate is null ? query : query.Where(predicate)).ToListAsync();
    }

    public override IEnumerable<Customer> GetAll(Expression<Func<Customer, bool>>? predicate = null) => FindAll(predicate);

    public override Task<IEnumerable<Customer>> GetAllAsync(Expression<Func<Customer, bool>>? predicate = null) => FindAllAsync(predicate);

    public override Customer? Find(int id)
        => Table.Include(x => x.PersonNavigation)
            .FirstOrDefault(x => x.Id == id);

    public override async Task<Customer?> FindAsync(int id)
        => await Table.Include(x => x.PersonNavigation)
            .FirstOrDefaultAsync(x => x.Id == id);
}
