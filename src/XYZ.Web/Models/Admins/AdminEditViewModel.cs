using System.ComponentModel.DataAnnotations;
using XYZ.Web.Infrastructure;

namespace XYZ.Web.Models.Admins
{
    public class AdminEditViewModel
    {
        [Required(ErrorMessage = ValidationMessages.Required)]
        public int Id { get; set; }

        [Display(Name = "Kullanıcı Id")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = ValidationMessages.Required)]
        [Display(Name = "Ad")]
        [StringLength(100, ErrorMessage = ValidationMessages.MaxLength)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = ValidationMessages.Required)]
        [Display(Name = "Ad")]
        [StringLength(50, ErrorMessage = ValidationMessages.MaxLength)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = ValidationMessages.Required)]
        [Display(Name = "Soyad")]
        [StringLength(50, ErrorMessage = ValidationMessages.MaxLength)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = ValidationMessages.Required)]
        [EmailAddress(ErrorMessage = ValidationMessages.Email)]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = ValidationMessages.Phone)]
        [Display(Name = "Telefon")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Kulüp")]
        public string TenantName { get; set; } = string.Empty;

        [Display(Name = "T.C. Kimlik No")]
        [RegularExpression("^[0-9]{11}$", ErrorMessage = ValidationMessages.TcIdentity)]
        public string? IdentityNumber { get; set; }

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
