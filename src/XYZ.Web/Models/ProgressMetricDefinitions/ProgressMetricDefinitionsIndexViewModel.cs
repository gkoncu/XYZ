using XYZ.Application.Features.ProgressMetricDefinitions.Queries;

namespace XYZ.Web.Models.ProgressMetricDefinitions
{
    public sealed class ProgressMetricDefinitionsIndexViewModel
    {
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;

        public bool IncludeInactive { get; set; }
        public List<ProgressMetricDefinitionListItemDto> Items { get; set; } = new();
    }
}
