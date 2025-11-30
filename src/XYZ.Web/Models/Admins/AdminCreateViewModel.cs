using System.ComponentModel.DataAnnotations;

namespace XYZ.Web.Models.Admins
{
    public class AdminCreateViewModel
    {
        [Required]
        [Display(Name = "Ad")]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Soyad")]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Telefon")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Kulüp Id (yalnızca SuperAdmin)")]
        public int? TenantId { get; set; }

        [Display(Name = "T.C. Kimlik No")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "TC Kimlik No 11 haneli olmalıdır.")]
        public string? IdentityNumber { get; set; }

        [Display(Name = "Kullanıcı Yönetimi Yetkisi")]
        public bool CanManageUsers { get; set; } = true;

        [Display(Name = "Finans Yönetimi Yetkisi")]
        public bool CanManageFinance { get; set; } = true;

        [Display(Name = "Ayarlar Yönetimi Yetkisi")]
        public bool CanManageSettings { get; set; } = true;
    }
}
