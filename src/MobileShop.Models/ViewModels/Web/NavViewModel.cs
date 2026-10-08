namespace MobileShop.Models.ViewModels.Web;

public sealed class NavViewModel
{
    public required IReadOnlyList<NavItem> Items { get; init; }
}

public sealed class NavItem
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required string IconCssClass { get; init; }
    public string? ControllerName { get; init; }
    public string? ActionName { get; init; }
    public string? PageName { get; init; }
    public string? Url { get; init; }
    public IReadOnlyList<string> RequiredRoles { get; init; } = [];
    public bool IsActive { get; init; }
    public IReadOnlyList<NavSubItem> SubItems { get; init; } = [];
}

public sealed record NavSubItem
{
    public required string Title { get; init; }
    public required string Description { get; init; }
    public string? BadgeText { get; init; }
    public required string Url { get; init; }
    public required string PageName { get; init; }
    public required string IconCssClass { get; init; }
    public bool IsActive { get; init; }
}
