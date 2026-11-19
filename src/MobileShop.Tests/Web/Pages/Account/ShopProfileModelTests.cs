using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MobileShop.Services.DataServices.Interfaces;
using MobileShop.Web.Pages.Account;
using Moq;

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
}
