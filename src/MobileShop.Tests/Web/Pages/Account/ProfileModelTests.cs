using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using MobileShop.Web.Pages.Account;
using Moq;

namespace MobileShop.Tests.Web.Pages.Account;

public class ProfileModelTests
{
    private readonly ProfileModel _model;
    private readonly Mock<IAccountDataService> _accountDataServiceMock = new();
    private readonly Mock<IAuthenticationService> _authenticationServiceMock = new();

    public ProfileModelTests()
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.Name, "admin")],
                CookieAuthenticationDefaults.AuthenticationScheme))
        };
        var tempDataFactory = new Mock<ITempDataDictionaryFactory>();
        tempDataFactory.Setup(factory => factory.GetTempData(httpContext))
            .Returns(new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>()));
        httpContext.RequestServices = new ServiceCollection()
            .AddSingleton(_authenticationServiceMock.Object)
            .AddSingleton(tempDataFactory.Object)
            .BuildServiceProvider();
        _model = new ProfileModel(_accountDataServiceMock.Object)
        {
            PageContext = new PageContext { HttpContext = httpContext }
        };
    }

    [Fact]
    public void OnGet_uses_the_authenticated_username()
    {
        _model.OnGet();

        Assert.Equal("admin", _model.Username);
        Assert.Equal("admin", _model.NewUsername);
    }

    [Fact]
    public async Task OnPostAsync_updates_username_and_optional_password_then_refreshes_cookie()
    {
        _model.CurrentPassword = "Admin@123";
        _model.NewUsername = "shop-owner";
        _model.NewPassword = "NewPass123";
        _model.ConfirmPassword = "NewPass123";
        _accountDataServiceMock
            .Setup(service => service.ChangeCredentialsAsync("admin", "Admin@123", "shop-owner", "NewPass123"))
            .ReturnsAsync(new ServiceResult(true, "Profile updated successfully."));

        var result = await _model.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.Equal("shop-owner", _model.Username);
        Assert.Equal("shop-owner", _model.NewUsername);
        Assert.Equal("Profile updated successfully.", _model.Message);
        Assert.Null(_model.CurrentPassword);
        Assert.Null(_model.NewPassword);
        _authenticationServiceMock.Verify(service => service.SignInAsync(
            It.IsAny<HttpContext>(),
            CookieAuthenticationDefaults.AuthenticationScheme,
            It.Is<ClaimsPrincipal>(principal => principal.Identity!.Name == "shop-owner"),
            It.IsAny<AuthenticationProperties?>()), Times.Once);
    }

    [Fact]
    public async Task OnPostAsync_preserves_the_profile_when_service_rejects_current_password()
    {
        _model.CurrentPassword = "wrong";
        _model.NewUsername = "admin";
        _accountDataServiceMock
            .Setup(service => service.ChangeCredentialsAsync("admin", "wrong", "admin", null))
            .ReturnsAsync(new ServiceResult(false, "Invalid current password."));

        var result = await _model.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.Contains(_model.ModelState[string.Empty]!.Errors,
            error => error.ErrorMessage == "Invalid current password.");
        _authenticationServiceMock.Verify(service => service.SignInAsync(
            It.IsAny<HttpContext>(), It.IsAny<string?>(), It.IsAny<ClaimsPrincipal>(),
            It.IsAny<AuthenticationProperties?>()), Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_rejects_mismatched_passwords()
    {
        _model.CurrentPassword = "Admin@123";
        _model.NewUsername = "admin";
        _model.NewPassword = "NewPass123";
        _model.ConfirmPassword = "DifferentPass";
        ValidateModel(_model);

        var result = await _model.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.False(_model.ModelState.IsValid);
        Assert.Contains(_model.ModelState[nameof(ProfileModel.ConfirmPassword)]!.Errors,
            error => error.ErrorMessage == "New passwords do not match.");
    }

    [Fact]
    public async Task OnPostAsync_rejects_short_new_password()
    {
        _model.CurrentPassword = "Admin@123";
        _model.NewUsername = "admin";
        _model.NewPassword = "short";
        _model.ConfirmPassword = "short";
        ValidateModel(_model);

        var result = await _model.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.False(_model.ModelState.IsValid);
        Assert.Contains(_model.ModelState[nameof(ProfileModel.NewPassword)]!.Errors,
            error => error.ErrorMessage!.Contains("at least 6 characters"));
    }

    private static void ValidateModel(ProfileModel model)
    {
        foreach (var property in typeof(ProfileModel).GetProperties())
        {
            if (!property.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.ValidationAttribute), true).Any())
                continue;

            var context = new System.ComponentModel.DataAnnotations.ValidationContext(model)
            {
                MemberName = property.Name
            };
            var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            if (!System.ComponentModel.DataAnnotations.Validator.TryValidateProperty(
                    property.GetValue(model), context, results))
            {
                foreach (var result in results)
                    model.ModelState.AddModelError(property.Name, result.ErrorMessage ?? string.Empty);
            }
        }
    }
}
