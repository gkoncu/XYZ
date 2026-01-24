using Microsoft.AspNetCore.Mvc.Rendering;
using XYZ.Application.Features.Documents.Queries.DocumentStatus;
using XYZ.Application.Features.Documents.Queries.GetUserDocuments;

namespace XYZ.Web.Models.Documents
{
    public class UserDocumentsViewModel
    {
        public string Title { get; set; } = string.Empty;

        public int OwnerId { get; set; }
        public string OwnerDisplayName { get; set; } = string.Empty;

        public UserDocumentStatusDto Status { get; set; } = new();

        public IList<DocumentListItemDto> Documents { get; set; } = new List<DocumentListItemDto>();

        public int? TypeFilter { get; set; }

        public int DocumentDefinitionId { get; set; }
        public IFormFile? File { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool IsCoach { get; set; }

        public IList<SelectListItem> DocumentDefinitionOptions { get; set; } = new List<SelectListItem>();
    }
}
