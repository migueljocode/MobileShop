using Microsoft.AspNetCore.Mvc.RazorPages;
using MobileShop.Services.DataServices.Interfaces;
using MobileShop.Web.Pages.People;
using Moq;

namespace MobileShop.Tests.Web.Pages.People;

/// <summary>Verifies sort-parameter handling on the Customers list page.</summary>
public class CustomersModelTests
{
    private readonly Mock<ICustomerDataService> _serviceMock;
    private readonly CustomersModel _model;

    public CustomersModelTests()
    {
        _serviceMock = new Mock<ICustomerDataService>();
        _serviceMock
            .Setup(service => service.GetListRowsAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync([]);
        _model = new CustomersModel(_serviceMock.Object);
    }

    [Fact]
    public async Task OnGetAsync_DefaultsToNameAscending()
    {
        await _model.OnGetAsync();

        Assert.Equal("Name", _model.SortBy);
        Assert.True(_model.Ascending);
        _serviceMock.Verify(service => service.GetListRowsAsync("Name", true), Times.Once);
    }

    [Fact]
    public async Task OnGetAsync_PassesExplicitSortParamsThrough()
    {
        await _model.OnGetAsync("Count", ascending: false);

        Assert.Equal("Count", _model.SortBy);
        Assert.False(_model.Ascending);
        _serviceMock.Verify(service => service.GetListRowsAsync("Count", false), Times.Once);
    }

    [Fact]
    public async Task OnGetAsync_WithInvalidSortBy_KeepsRawValueForHeaderToggleState()
    {
        await _model.OnGetAsync("Bogus");

        Assert.Equal("Bogus", _model.SortBy);
        Assert.True(_model.Ascending);
        _serviceMock.Verify(service => service.GetListRowsAsync("Bogus", true), Times.Once);
    }
}