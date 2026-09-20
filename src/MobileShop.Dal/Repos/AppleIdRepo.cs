namespace MobileShop.Dal.Repos;

/// <inheritdoc cref="IAppleIdRepo" />
public class AppleIdRepo(AppDbContext context) : BaseRepo<AppleId>(context), IAppleIdRepo
{
    public AppleId? Find(string email)
        => Table.FirstOrDefault(x => x.Email.ToLower() == email.ToLower());

    public async Task<AppleId?> FindAsync(string email)
        => await Table.FirstOrDefaultAsync(x => x.Email.ToLower() == email.ToLower());
}
