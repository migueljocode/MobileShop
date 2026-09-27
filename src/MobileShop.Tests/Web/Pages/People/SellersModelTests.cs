using Microsoft.AspNetCore.Mvc.RazorPages;
using MobileShop.Services.DataServices.Interfaces;
using MobileShop.Web.Pages.People;
using Moq;

namespace MobileShop.Tests.Web.Pages.People;

/// <summary>Verifies sort-parameter handling on the Sellers list page.</summary>
public class SellersModelTests
{
    private readonly Mock<ISellerDataService> _serviceMock;
    private readonly SellersModel _model;

    public SellersModelTests()
    {
        _serviceMock = new Mock<ISellerDataService>();
        _serviceMock
            .Setup(service => service.GetListRowsAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync([]);
        _model = new SellersModel(_serviceMock.Object);
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
        await _model.OnGetAsync("Phone", ascending: false);

        Assert.Equal("Phone", _model.SortBy);
        Assert.False(_model.Ascending);
        _serviceMock.Verify(service => service.GetListRowsAsync("Phone", false), Times.Once);
    }

    [Fact]
    public async Task OnGetAsync_DefaultAscending_WhenQueryOmitsParam()
    {
        // Simulates the ASP.NET binding caveat: omitting the bool query param in a unit call
        // still lands on the method's declared default of true.
        await _model.OnGetAsync("Name");

        Assert.True(_model.Ascending);
        _serviceMock.Verify(service => service.GetListRowsAsync("Name", true), Times.Once);
    }
}