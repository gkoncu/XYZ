using XYZ.Application.Features.DocumentDefinitions.Queries;
using XYZ.Application.Features.DocumentDefinitions.Queries.GetDocumentDefinitions;

namespace XYZ.Web.Models.DocumentDefinitions
{
    public class DocumentDefinitionsIndexViewModel
    {
        public int Target { get; set; }
        public bool IncludeInactive { get; set; }

        public string Title { get; set; } = "Belge Türleri";

        public IList<DocumentDefinitionListItemDto> Items { get; set; } = new List<DocumentDefinitionListItemDto>();
    }
}
