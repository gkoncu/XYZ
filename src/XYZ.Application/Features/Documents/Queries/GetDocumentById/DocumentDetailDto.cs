using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Documents.Queries.GetDocumentById
{
    public class DocumentDetailDto
    {
        public int Id { get; set; }

        public int StudentId { get; set; }
        public string StudentFullName { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DocumentType Type { get; set; }

        public DateTime UploadDate { get; set; }
        public string UploadedBy { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }
}
