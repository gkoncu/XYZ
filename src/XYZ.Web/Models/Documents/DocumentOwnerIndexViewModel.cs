using XYZ.Application.Features.Documents.Queries.DocumentStatus;

namespace XYZ.Web.Models.Documents
{
    public class DocumentOwnerIndexViewModel<TItem>
    {
        public string Title { get; set; } = string.Empty;

        public bool OnlyIncomplete { get; set; } = true;
        public string? SearchTerm { get; set; }

        public IList<TItem> Items { get; set; } = new List<TItem>();
    }
}
