using MobileShop.Web.ViewModels;

namespace MobileShop.Web.ViewComponents;

public sealed class NavigationMenuViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        var routeData = ViewContext.RouteData.Values;
        var currentPage = routeData["page"]?.ToString();
        var currentController = routeData["controller"]?.ToString();
        var currentAction = routeData["action"]?.ToString();
        var items = new List<NavItem>
        {
            CreateItem("dashboard", "Dashboard", "nav-icon-home", "/Index", "/Index",
                [Link("Overview", "Your shop at a glance.", "/Index", "nav-subicon-home")]),
            CreateItem("products", "Products", "nav-icon-products", "/Products", "/Products/Index",
                [
                    Link("All products", "Browse and manage the inventory.", "/Products/Index", "nav-subicon-grid"),
                    Link("Second-hand", "Review pre-owned stock.", "/Products/SecondHand", "nav-subicon-reuse"),
                    Link("Add a phone", "Register a new phone in stock.", "/Products/CreatePhone", "nav-subicon-plus"),
                    Link("Add an Apple ID", "Register a new Apple ID in stock.", "/Products/CreateAppleId", "nav-subicon-plus"),
                    Link("Add a glass", "Register a new glass accessory.", "/Products/CreateGlass", "nav-subicon-plus"),
                    Link("Add a case", "Register a new phone case.", "/Products/CreateCase", "nav-subicon-plus"),
                    Link("Add a tablet", "Register a new tablet.", "/Products/CreateTablet", "nav-subicon-plus"),
                    Link("Add a smart watch", "Register a new smart watch.", "/Products/CreateSmartWatch", "nav-subicon-plus"),
                    Link("Add a laptop", "Register a new laptop.", "/Products/CreateLaptop", "nav-subicon-plus"),
                    Link("Add a cable", "Register a new cable accessory.", "/Products/CreateCable", "nav-subicon-plus"),
                    Link("Add a charger", "Register a new charger.", "/Products/CreateCharger", "nav-subicon-plus"),
                    Link("Add a power bank", "Register a new power bank.", "/Products/CreatePowerBank", "nav-subicon-plus"),
                    Link("Add portable storage", "Register portable storage in stock.", "/Products/CreatePortableStorage", "nav-subicon-plus")
                ]),
            CreateItem("transactions", "Transactions", "nav-icon-transactions", "/Transactions", "/Transactions/Index",
                [
                    Link("All transactions", "View the purchase and sales history.", "/Transactions/Index", "nav-subicon-grid"),
                    Link("Record a sale", "Record an item sold to a customer.", "/Transactions/Sell", "nav-subicon-arrow-up")
                ]),
            CreateItem("people", "People", "nav-icon-people", "/People", "/People/Index",
                [
                    Link("All people", "Browse customers and sellers.", "/People/Index", "nav-subicon-people"),
                    Link("Customers", "View your customer list.", "/People/Customers", "nav-subicon-person"),
                    Link("Sellers", "View your seller list.", "/People/Sellers", "nav-subicon-person")
                ]),
            CreateItem("reports", "Reports", "nav-icon-reports", "/Reports", "/Reports/ProfitLoss",
                [
                    Link("Profit & loss", "Review the shop's financial performance.", "/Reports/ProfitLoss", "nav-subicon-chart")
                ]),
            CreateItem("account", "Profile", "nav-icon-account", "/Account", "/Account/Profile",
                [
                    Link("Profile", "Manage your account profile.", "/Account/Profile", "nav-subicon-person"),
                    Link("Sign out", "End your current session.", "/Account/Logout", "nav-subicon-exit")
                ])
        };

        items = items
            .Where(item => IsVisibleToCurrentUser(item))
            .Select(item => SetRouteState(item, currentPage, currentController, currentAction))
            .ToList();

        return View(new NavViewModel { Items = items });
    }

    private bool IsVisibleToCurrentUser(NavItem item) =>
        item.RequiredRoles.Count == 0 || item.RequiredRoles.Any(User.IsInRole);

    private NavItem CreateItem(
        string id,
        string title,
        string iconCssClass,
        string pagePrefix,
        string destinationPage,
        IReadOnlyList<NavSubItem> subItems) =>
        new()
        {
            Id = id,
            Title = title,
            IconCssClass = iconCssClass,
            ControllerName = id switch
            {
                "dashboard" => "Dashboard",
                "products" => "Products",
                "transactions" => "Transactions",
                "people" => "People",
                "reports" => "Reports",
                "account" => "Account",
                _ => null
            },
            PageName = pagePrefix,
            Url = Url.Page(destinationPage) ??
                  throw new InvalidOperationException($"The Razor Page '{destinationPage}' could not be resolved."),
            SubItems = subItems
        };

    private NavSubItem Link(
        string title,
        string description,
        string pageName,
        string iconCssClass,
        string? badgeText = null) =>
        new()
        {
            Title = title,
            Description = description,
            Url = Url.Page(pageName) ?? throw new InvalidOperationException($"The Razor Page '{pageName}' could not be resolved."),
            PageName = pageName,
            IconCssClass = iconCssClass,
            BadgeText = badgeText
        };

    private static NavItem SetRouteState(
        NavItem item,
        string? currentPage,
        string? currentController,
        string? currentAction)
    {
        var isPageActive = currentPage is not null &&
                           (string.Equals(currentPage, item.PageName, StringComparison.OrdinalIgnoreCase) ||
                            (item.PageName != "/Index" &&
                             currentPage.StartsWith(item.PageName + "/", StringComparison.OrdinalIgnoreCase)));
        var isControllerActive = !string.IsNullOrWhiteSpace(currentController) &&
                                 string.Equals(currentController, item.ControllerName, StringComparison.OrdinalIgnoreCase) &&
                                 (string.IsNullOrWhiteSpace(item.ActionName) ||
                                  string.Equals(currentAction, item.ActionName, StringComparison.OrdinalIgnoreCase));
        var subItems = item.SubItems
            .Select(subItem => subItem with
            {
                IsActive = string.Equals(currentPage, subItem.PageName, StringComparison.OrdinalIgnoreCase)
            })
            .ToList();

        return new NavItem
        {
            Id = item.Id,
            Title = item.Title,
            IconCssClass = item.IconCssClass,
            ControllerName = item.ControllerName,
            ActionName = item.ActionName,
            PageName = item.PageName,
            Url = item.Url,
            RequiredRoles = item.RequiredRoles,
            IsActive = isPageActive || isControllerActive,
            SubItems = subItems
        };
    }
}
