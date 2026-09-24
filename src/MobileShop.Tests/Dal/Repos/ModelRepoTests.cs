using MobileShop.Dal.Repos;
using MobileShop.Models.Entities;

namespace MobileShop.Tests.Dal.Repos;

public class ModelRepoTests : RepoTestBase
{
    private readonly ModelRepo _repo;

    public ModelRepoTests()
    {
        _repo = new ModelRepo(Context);
    }

    [Fact]
    public void Add_and_Find_works_for_model()
    {
        var manufacturer = new Manufacturer { Name = "Test Mfr" };
        Context.Manufacturers.Add(manufacturer);
        Context.SaveChanges();

        var category = new Category { Name = "Test Cat" };
        Context.Categories.Add(category);
        Context.SaveChanges();

        var model = new Model { Name = "Test Model", ManufacturerId = manufacturer.Id, CategoryId = category.Id };
        _repo.Add(model);
        Context.SaveChanges();

        var found = _repo.Find(model.Id);
        Assert.NotNull(found);
        Assert.Equal("Test Model", found!.Name);
    }

    [Fact]
    public void Find_returns_null_for_nonexistent_model()
    {
        var found = _repo.Find(99999);
        Assert.Null(found);
    }

    [Fact]
    public void FindAll_returns_all_models()
    {
        var manufacturer = new Manufacturer { Name = "Test Mfr" };
        Context.Manufacturers.Add(manufacturer);
        Context.SaveChanges();

        var category = new Category { Name = "Test Cat" };
        Context.Categories.Add(category);
        Context.SaveChanges();

        _repo.Add(new Model { Name = "Model 1", ManufacturerId = manufacturer.Id, CategoryId = category.Id });
        _repo.Add(new Model { Name = "Model 2", ManufacturerId = manufacturer.Id, CategoryId = category.Id });
        Context.SaveChanges();

        var all = _repo.FindAll().ToList();
        Assert.Equal(2, all.Count);
    }

    [Fact]
    public void Delete_works_for_model()
    {
        var manufacturer = new Manufacturer { Name = "Test Mfr" };
        Context.Manufacturers.Add(manufacturer);
        Context.SaveChanges();

        var category = new Category { Name = "Test Cat" };
        Context.Categories.Add(category);
        Context.SaveChanges();

        var model = new Model { Name = "To Remove", ManufacturerId = manufacturer.Id, CategoryId = category.Id };
        _repo.Add(model);
        Context.SaveChanges();

        _repo.Delete(model);
        Context.SaveChanges();

        var found = _repo.Find(model.Id);
        Assert.Null(found);
    }

    [Fact]
    public void Any_returns_true_for_existing_model()
    {
        var manufacturer = new Manufacturer { Name = "Test Mfr" };
        Context.Manufacturers.Add(manufacturer);
        Context.SaveChanges();

        var category = new Category { Name = "Test Cat" };
        Context.Categories.Add(category);
        Context.SaveChanges();

        var model = new Model { Name = "Exists", ManufacturerId = manufacturer.Id, CategoryId = category.Id };
        _repo.Add(model);
        Context.SaveChanges();

        Assert.True(_repo.Any(m => m.Id == model.Id));
    }

    [Fact]
    public void Any_returns_false_for_nonexistent_model()
    {
        Assert.False(_repo.Any(m => m.Id == 99999));
    }
}
