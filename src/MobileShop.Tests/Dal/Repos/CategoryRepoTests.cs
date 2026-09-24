using MobileShop.Dal.Repos;
using MobileShop.Models.Entities;

namespace MobileShop.Tests.Dal.Repos;

public class CategoryRepoTests : RepoTestBase
{
    private readonly CategoryRepo _repo;

    public CategoryRepoTests()
    {
        _repo = new CategoryRepo(Context);
    }

    [Fact]
    public void Add_and_Find_works_for_category()
    {
        var category = new Category { Name = "Test Category" };
        _repo.Add(category);
        Context.SaveChanges();

        var found = _repo.Find(category.Id);
        Assert.NotNull(found);
        Assert.Equal("Test Category", found!.Name);
    }

    [Fact]
    public void Find_returns_null_for_nonexistent_category()
    {
        var found = _repo.Find(99999);
        Assert.Null(found);
    }

    [Fact]
    public void FindAll_returns_all_categories()
    {
        _repo.Add(new Category { Name = "Cat1" });
        _repo.Add(new Category { Name = "Cat2" });
        Context.SaveChanges();

        var all = _repo.FindAll().ToList();
        Assert.Equal(2, all.Count);
    }

    [Fact]
    public void Delete_works_for_category()
    {
        var category = new Category { Name = "To Remove" };
        _repo.Add(category);
        Context.SaveChanges();

        _repo.Delete(category);
        Context.SaveChanges();

        var found = _repo.Find(category.Id);
        Assert.Null(found);
    }

    [Fact]
    public void Any_returns_true_for_existing_category()
    {
        var category = new Category { Name = "Exists" };
        _repo.Add(category);
        Context.SaveChanges();

        Assert.True(_repo.Any(c => c.Id == category.Id));
    }

    [Fact]
    public void Any_returns_false_for_nonexistent_category()
    {
        Assert.False(_repo.Any(c => c.Id == 99999));
    }
}
