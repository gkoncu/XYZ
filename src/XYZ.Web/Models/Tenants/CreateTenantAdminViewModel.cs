using System.ComponentModel.DataAnnotations;

namespace XYZ.Web.Models.Tenants
{
    public sealed class CreateTenantAdminViewModel
    {
        [Required(ErrorMessage = "Ad zorunludur.")]
        [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir.")]
        [Display(Name = "Ad")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad zorunludur.")]
        [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir.")]
        [Display(Name = "Soyad")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "E-posta formatı geçersiz.")]
        [StringLength(256, ErrorMessage = "E-posta en fazla 256 karakter olabilir.")]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Telefon formatı geçersiz.")]
        [StringLength(20, ErrorMessage = "Telefon en fazla 20 karakter olabilir.")]
        [Display(Name = "Telefon")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "T.C. Kimlik No")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "T.C. Kimlik No 11 haneli ve sadece rakam olmalıdır.")]
        public string? IdentityNumber { get; set; }

        [Display(Name = "Kullanıcı Yönetimi")]
        public bool CanManageUsers { get; set; } = true;

        [Display(Name = "Finans Yönetimi")]
        public bool CanManageFinance { get; set; } = true;

        [Display(Name = "Ayar Yönetimi")]
        public bool CanManageSettings { get; set; } = true;
    }
}
