using MobileShop.Dal.Repos;
using MobileShop.Models.Entities;

namespace MobileShop.Tests.Dal.Repos;

public class ColorRepoTests : RepoTestBase
{
    private readonly ColorRepo _repo;

    public ColorRepoTests()
    {
        _repo = new ColorRepo(Context);
    }

    [Fact]
    public void Add_and_Find_works_for_color()
    {
        var color = new Color { Name = "Test Color" };
        _repo.Add(color);
        Context.SaveChanges();

        var found = _repo.Find(color.Id);
        Assert.NotNull(found);
        Assert.Equal("Test Color", found!.Name);
    }

    [Fact]
    public void Find_returns_null_for_nonexistent_color()
    {
        var found = _repo.Find(99999);
        Assert.Null(found);
    }

    [Fact]
    public void FindAll_returns_all_colors()
    {
        _repo.Add(new Color { Name = "Red" });
        _repo.Add(new Color { Name = "Blue" });
        Context.SaveChanges();

        var all = _repo.FindAll().ToList();
        Assert.Equal(2, all.Count);
    }

    [Fact]
    public void Delete_works_for_color()
    {
        var color = new Color { Name = "To Remove" };
        _repo.Add(color);
        Context.SaveChanges();

        _repo.Delete(color);
        Context.SaveChanges();

        var found = _repo.Find(color.Id);
        Assert.Null(found);
    }

    [Fact]
    public void Any_returns_true_for_existing_color()
    {
        var color = new Color { Name = "Exists" };
        _repo.Add(color);
        Context.SaveChanges();

        Assert.True(_repo.Any(c => c.Id == color.Id));
    }

    [Fact]
    public void Any_returns_false_for_nonexistent_color()
    {
        Assert.False(_repo.Any(c => c.Id == 99999));
    }
}
