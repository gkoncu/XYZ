using System.ComponentModel.DataAnnotations;

namespace XYZ.Web.Models.Account;

public sealed class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "E-posta zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta girin.")]
    public string Email { get; set; } = string.Empty;

    public string? InfoMessage { get; set; }
}
