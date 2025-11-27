using System.ComponentModel.DataAnnotations;

namespace XYZ.Web.Models.Admins
{
    public class AdminEditViewModel
    {
        [Required]
        public int Id { get; set; }

        [Display(Name = "Kullanıcı Id")]
        public string UserId { get; set; } = string.Empty;

        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Telefon")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Tenant")]
        public string TenantName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "T.C. Kimlik No")]
        [StringLength(11, MinimumLength = 8)]
        public string IdentityNumber { get; set; } = string.Empty;

        [Display(Name = "Kullanıcı Yönetimi Yetkisi")]
        public bool CanManageUsers { get; set; }

        [Display(Name = "Finans Yönetimi Yetkisi")]
        public bool CanManageFinance { get; set; }

        [Display(Name = "Ayarlar Yönetimi Yetkisi")]
        public bool CanManageSettings { get; set; }

        [Display(Name = "Aktif mi?")]
        public bool IsActive { get; set; }
    }
}
