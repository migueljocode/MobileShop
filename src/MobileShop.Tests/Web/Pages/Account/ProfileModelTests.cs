using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MobileShop.Web.Pages.Account;
using Moq;

namespace MobileShop.Tests.Web.Pages.Account;

/// <summary>
/// Verifies the development profile page password change flow:
/// - GET resolves the seeded dev admin explicitly (no User.Identity dependency)
/// - POST validates current password, new password confirmation, and required inputs
/// - Successful change calls UserDataService.ChangePasswordAsync and displays success message
/// - Validation errors are preserved and displayed on redisplay
/// </summary>
public class ProfileModelTests : RepoTestBase
{
    private readonly ProfileModel _model;
    private readonly Mock<IUserDataService> _userDataServiceMock;

    public ProfileModelTests()
    {
        _userDataServiceMock = new Mock<IUserDataService>();
        _userDataServiceMock.Setup(u => u.FindByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);
        _userDataServiceMock.Setup(u => u.ValidateCredentialsAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(false);
        _userDataServiceMock.Setup(u => u.ChangePasswordAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        _userDataServiceMock.Setup(u => u.FindByUsernameAsync("admin"))
            .ReturnsAsync(new User { Id = 1, Username = "admin" });
        _userDataServiceMock.Setup(u => u.ValidateCredentialsAsync("admin", "Admin@123"))
            .ReturnsAsync(true);
        _userDataServiceMock.Setup(u => u.ValidateCredentialsAsync("admin", "wrong"))
            .ReturnsAsync(false);
        _userDataServiceMock.Setup(u => u.ChangePasswordAsync("admin", "NewPass123"))
            .ReturnsAsync(true);

        _model = new ProfileModel(_userDataServiceMock.Object);
    }

    [Fact]
    public async Task OnGetAsync_resolves_dev_admin_profile()
    {
        await _model.OnGetAsync();

        Assert.Equal("admin", _model.Username);
    }

    [Fact]
    public async Task OnPostAsync_changes_password_and_shows_success_message()
    {
        _model.CurrentPassword = "Admin@123";
        _model.NewPassword = "NewPass123";
        _model.ConfirmPassword = "NewPass123";

        var result = await _model.OnPostAsync();

        var pageResult = Assert.IsType<PageResult>(result);
        Assert.Equal("Password changed successfully.", _model.Message);
        Assert.Null(_model.CurrentPassword);
        Assert.Null(_model.NewPassword);
        Assert.Null(_model.ConfirmPassword);
        _userDataServiceMock.Verify(u => u.ChangePasswordAsync("admin", "NewPass123"), Times.Once);
    }

    [Fact]
    public async Task OnPostAsync_rejects_invalid_current_password()
    {
        _model.CurrentPassword = "wrong";
        _model.NewPassword = "NewPass123";
        _model.ConfirmPassword = "NewPass123";

        var result = await _model.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.False(_model.ModelState.IsValid);
        Assert.Contains(_model.ModelState[string.Empty]!.Errors, e => e.ErrorMessage == "Invalid current password.");
    }

    [Fact]
    public async Task OnPostAsync_rejects_mismatched_new_passwords()
    {
        _model.CurrentPassword = "Admin@123";
        _model.NewPassword = "NewPass123";
        _model.ConfirmPassword = "DifferentPass";

        var result = await _model.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.False(_model.ModelState.IsValid);
        Assert.Contains(_model.ModelState[string.Empty]!.Errors, e => e.ErrorMessage == "New passwords do not match.");
    }

    /// <summary>
    /// Runs data-annotation validation against the model the way MVC's model binder
    /// would, populating ModelState so attribute-driven errors can be asserted.
    /// </summary>
    private static void ValidateModel(ProfileModel model)
    {
        foreach (var prop in typeof(ProfileModel).GetProperties())
        {
            var value = prop.GetValue(model);
            var ctx = new System.ComponentModel.DataAnnotations.ValidationContext(model)
            {
                MemberName = prop.Name
            };
            var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            if (!System.ComponentModel.DataAnnotations.Validator.TryValidateProperty(value, ctx, results))
            {
                foreach (var r in results)
                {
                    model.ModelState.AddModelError(prop.Name, r.ErrorMessage ?? string.Empty);
                }
            }
        }
    }

    [Fact]
    public async Task OnPostAsync_rejects_short_new_password()
    {
        _model.CurrentPassword = "Admin@123";
        _model.NewPassword = "short";
        _model.ConfirmPassword = "short";
        ValidateModel(_model);

        var result = await _model.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.False(_model.ModelState.IsValid);
        Assert.Contains(_model.ModelState[nameof(ProfileModel.NewPassword)]!.Errors, 
            e => e.ErrorMessage.Contains("at least 6 characters"));
    }

    [Fact]
    public async Task OnPostAsync_rejects_missing_new_password()
    {
        _model.CurrentPassword = "Admin@123";
        _model.NewPassword = "";
        _model.ConfirmPassword = "";
        ValidateModel(_model);

        var result = await _model.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.False(_model.ModelState.IsValid);
        Assert.Contains(_model.ModelState[nameof(ProfileModel.NewPassword)]!.Errors, 
            e => e.ErrorMessage == "The NewPassword field is required.");
    }
}