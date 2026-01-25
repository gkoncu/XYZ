namespace XYZ.Web.Models.Settings;

public sealed class SettingsViewModel
{
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
    public string? TenantName { get; set; }
}
