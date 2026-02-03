using Microsoft.AspNetCore.Mvc.Rendering;

namespace XYZ.Web.Models.Settings;

public sealed class ProgressMetricsSettingsViewModel
{
    public int? SelectedBranchId { get; set; }

    public List<SelectListItem> BranchOptions { get; set; } = new();
}
