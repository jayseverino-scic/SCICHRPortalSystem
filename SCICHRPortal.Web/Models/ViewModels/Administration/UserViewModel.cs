using System.ComponentModel.DataAnnotations;
using SCICHRPortal.Data.DTOs;

namespace SCICHRPortal.Web.Models.ViewModels.Administration;

public class UserProfileViewModel : UserProfileRequest { }

public sealed class UserViewModel : UserProfileViewModel
{
    public int UserId { get; set; }
    [Required, MinLength(1), Display(Name = "Roles")]
    public int[] RoleIds { get; set; } = [];
}

public sealed class UserPasswordViewModel
{
    [Required, MaxLength(255), DataType(DataType.Password), Display(Name = "Current password")]
    public string? CurrentPassword { get; set; }
    [Required, StringLength(255, MinimumLength = 8), RegularExpression("^(?=.*\\d)(?=.*[a-zA-Z]).+$"), DataType(DataType.Password)]
    public string? Password { get; set; }
    [Required, Compare(nameof(Password)), DataType(DataType.Password), Display(Name = "Confirm password")]
    public string? ConfirmPassword { get; set; }
}
