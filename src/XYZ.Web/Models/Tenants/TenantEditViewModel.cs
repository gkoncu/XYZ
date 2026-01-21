using System.ComponentModel.DataAnnotations;

namespace XYZ.Web.Models.Tenants
{
    public class TenantEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Tenant Adı")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Subdomain")]
        public string Subdomain { get; set; } = string.Empty;

        [Display(Name = "Adres")]
        public string? Address { get; set; }

        [Display(Name = "Telefon")]
        public string? Phone { get; set; }

        [EmailAddress]
        [Display(Name = "E-posta")]
        public string? Email { get; set; }

        [Display(Name = "Logo Url")]
        public string? LogoUrl { get; set; }

        [Display(Name = "Primary Color")]
        public string? PrimaryColor { get; set; }

        [Display(Name = "Secondary Color")]
        public string? SecondaryColor { get; set; }

        [Display(Name = "Subscription Plan")]
        public string? SubscriptionPlan { get; set; }

        [Display(Name = "Başlangıç")]
        public DateOnly? SubscriptionStartDate { get; set; }

        [Display(Name = "Bitiş")]
        public DateOnly? SubscriptionEndDate { get; set; }

        [Display(Name = "Aktif mi?")]
        public bool IsActive { get; set; }
    }
}
