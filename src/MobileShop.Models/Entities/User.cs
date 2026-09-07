namespace MobileShop.Models.Entities;

[Table("Users")]
public class User : BaseEntity
{
    [Required(ErrorMessage = "{0} is required.")]
    [StringLength(50, ErrorMessage = "{0} cannot exceed {1} characters.")]
    public string Username { get; set; } = string.Empty;

    // store a hash here, never the plaintext password - unlike AppleId.Password, which has to stay
    // retrievable since it's a real external credential, not a login check against this app itself
    [Required(ErrorMessage = "{0} is required.")]
    [StringLength(200, ErrorMessage = "{0} cannot exceed {1} characters.")]
    public string PasswordHash { get; set; } = string.Empty;

    public int PersonId { get; set; }
    public virtual Person PersonNavigation { get; set; } = null!;
}
