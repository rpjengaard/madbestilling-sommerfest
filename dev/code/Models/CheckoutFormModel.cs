using System.ComponentModel.DataAnnotations;

namespace Madbestilling.Models;

public class CheckoutFormModel
{
    [Required]
    public string ChildName { get; set; } = string.Empty;

    [Required]
    public string ChildClass { get; set; } = string.Empty;

    [Required]
    public string Phone { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string EmailConfirm { get; set; } = string.Empty;

    [Required]
    public string CartJson { get; set; } = string.Empty;

    public string MobilePayBoxNumber { get; set; } = string.Empty;

    public string ReturnUrl { get; set; } = string.Empty;
}
