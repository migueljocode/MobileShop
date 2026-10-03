namespace MobileShop.Models.ViewModels.Web.BindModels;

/// <summary>Input submitted when creating an Apple ID product.</summary>
public sealed class CreateAppleIdInputModel
{
    [Range(0, MoneyLimits.MaxRials)]
    public long Price { get; set; }

    [Range(0, 100)]
    public decimal? ProfitPercent { get; set; }

    [Range(0, MoneyLimits.MaxRials)]
    public long? ProfitAmount { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Password { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Notes { get; set; }
}
