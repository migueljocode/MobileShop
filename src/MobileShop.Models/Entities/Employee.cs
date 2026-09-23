namespace MobileShop.Models.Entities;

[Table("Employees")]
public class Employee : BaseEntity
{
    [Required(ErrorMessage = "{0} is required.")]
    public int PersonId { get; set; }

    public virtual Person PersonNavigation { get; set; } = null!;

    [Range(0, 100, ErrorMessage = "Share percent must be between 0 and 100.")]
    public int SharePercent { get; set; }

    [Required(ErrorMessage = "{0} is required.")]
    public DateTime HireDate { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    [StringLength(500, ErrorMessage = "{0} cannot exceed {1} characters.")]
    public string? Notes { get; set; }
}