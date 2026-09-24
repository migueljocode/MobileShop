using MobileShop.Dal.Repos;
using MobileShop.Models.Entities;

namespace MobileShop.Tests.Dal.Repos;

public class ManufacturerRepoTests : RepoTestBase
{
    private readonly ManufacturerRepo _repo;

    public ManufacturerRepoTests()
    {
        _repo = new ManufacturerRepo(Context);
    }

    [Fact]
    public void Add_and_Find_works_for_manufacturer()
    {
        var manufacturer = new Manufacturer { Name = "Test Manufacturer" };
        _repo.Add(manufacturer);
        Context.SaveChanges();

        var found = _repo.Find(manufacturer.Id);
        Assert.NotNull(found);
        Assert.Equal("Test Manufacturer", found!.Name);
    }

    [Fact]
    public void Find_returns_null_for_nonexistent_manufacturer()
    {
        var found = _repo.Find(99999);
        Assert.Null(found);
    }

    [Fact]
    public void FindAll_returns_all_manufacturers()
    {
        _repo.Add(new Manufacturer { Name = "Apple" });
        _repo.Add(new Manufacturer { Name = "Samsung" });
        Context.SaveChanges();

        var all = _repo.FindAll().ToList();
        Assert.Equal(2, all.Count);
    }

    [Fact]
    public void Delete_works_for_manufacturer()
    {
        var manufacturer = new Manufacturer { Name = "To Remove" };
        _repo.Add(manufacturer);
        Context.SaveChanges();

        _repo.Delete(manufacturer);
        Context.SaveChanges();

        var found = _repo.Find(manufacturer.Id);
        Assert.Null(found);
    }

    [Fact]
    public void Any_returns_true_for_existing_manufacturer()
    {
        var manufacturer = new Manufacturer { Name = "Exists" };
        _repo.Add(manufacturer);
        Context.SaveChanges();

        Assert.True(_repo.Any(m => m.Id == manufacturer.Id));
    }

    [Fact]
    public void Any_returns_false_for_nonexistent_manufacturer()
    {
        Assert.False(_repo.Any(m => m.Id == 99999));
    }
}
