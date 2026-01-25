using System.ComponentModel.DataAnnotations;

namespace XYZ.Web.Models.Settings;

public sealed class ClubSettingsViewModel
{
    public string TenantName { get; set; } = "";

    [Required]
    [RegularExpression("^#([0-9a-fA-F]{6})$", ErrorMessage = "Renk '#RRGGBB' formatında olmalı.")]
    public string PrimaryColor { get; set; } = "#3B82F6";

    [Required]
    [RegularExpression("^#([0-9a-fA-F]{6})$", ErrorMessage = "Renk '#RRGGBB' formatında olmalı.")]
    public string SecondaryColor { get; set; } = "#1E40AF";

    [MaxLength(500)]
    public string? LogoUrl { get; set; }
}
