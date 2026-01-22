using System.ComponentModel.DataAnnotations;

namespace XYZ.Web.Models.Tenants
{
    public sealed class CreateTenantAdminViewModel
    {
        [Required]
        [Display(Name = "Ad")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Soyad")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Telefon")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "T.C. Kimlik No")]
        public string? IdentityNumber { get; set; }

        [Display(Name = "Kullanıcı Yönetimi")]
        public bool CanManageUsers { get; set; } = true;

        [Display(Name = "Finans Yönetimi")]
        public bool CanManageFinance { get; set; } = true;

        [Display(Name = "Ayar Yönetimi")]
        public bool CanManageSettings { get; set; } = true;
    }
}
