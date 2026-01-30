using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels;

public class ManageViewModel
{
    [Required]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [Phone]
    [Display(Name = "Phone number")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Address")]
    public string? Address { get; set; }

    [Display(Name = "City")]
    public string? City { get; set; }

    [Display(Name = "Postal code")]
    public string? PostalCode { get; set; }

    [Display(Name = "Country")]
    public string? Country { get; set; }
}
