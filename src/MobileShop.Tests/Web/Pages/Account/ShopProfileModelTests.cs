namespace MobileShop.Tests.Web.Pages.Account;

public class ShopProfileModelTests
{
    private readonly Mock<IShopProfileDataService> _serviceMock = new();
    private readonly ShopProfileModel _model;

    public ShopProfileModelTests()
    {
        _model = new ShopProfileModel(_serviceMock.Object)
        {
            PageContext = new PageContext { HttpContext = new DefaultHttpContext() }
        };
    }

    [Fact]
    public async Task OnGetAsync_loads_the_current_shop_profile()
    {
        _serviceMock
            .Setup(service => service.GetAsync())
            .ReturnsAsync(new ShopProfileView("Zone Mobile", "Main St", "09120000000", "@zonemobile"));

        await _model.OnGetAsync();

        Assert.Equal("Zone Mobile", _model.Input.Name);
        Assert.Equal("Main St", _model.Input.Address);
        Assert.Equal("09120000000", _model.Input.Phone);
        Assert.Equal("@zonemobile", _model.Input.Instagram);
    }

    [Fact]
    public async Task OnPostAsync_saves_valid_input_and_sets_success_message()
    {
        _model.Input = new ShopProfileInput
        {
            Name = "Zone Mobile",
            Address = "Main St",
            Phone = "09120000000",
            Instagram = "@zonemobile"
        };
        _serviceMock
            .Setup(service => service.SaveAsync(_model.Input))
            .ReturnsAsync(new ServiceResult(true, "Shop profile updated successfully."));

        var result = await _model.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.Equal("Shop profile updated successfully.", _model.Message);
        _serviceMock.Verify(service => service.SaveAsync(_model.Input), Times.Once);
    }

    [Fact]
    public async Task OnPostAsync_returns_page_without_saving_when_model_is_invalid()
    {
        _model.Input = new ShopProfileInput
        {
            Name = string.Empty,
            Phone = string.Empty
        };
        ValidateInput(_model);

        var result = await _model.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.False(_model.ModelState.IsValid);
        Assert.Null(_model.Message);
        _serviceMock.Verify(service => service.SaveAsync(It.IsAny<ShopProfileInput>()), Times.Never);
    }

    private static void ValidateInput(ShopProfileModel model)
    {
        foreach (var property in typeof(ShopProfileInput).GetProperties())
        {
            if (!property.GetCustomAttributes(typeof(ValidationAttribute), true).Any())
                continue;

            var context = new ValidationContext(model.Input)
            {
                MemberName = property.Name
            };
            var results = new List<ValidationResult>();
            if (!Validator.TryValidateProperty(property.GetValue(model.Input), context, results))
            {
                foreach (var validationResult in results)
                    model.ModelState.AddModelError($"{nameof(ShopProfileModel.Input)}.{property.Name}", validationResult.ErrorMessage ?? string.Empty);
            }
        }
    }
}
