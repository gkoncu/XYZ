using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using XYZ.Domain.Common;
using XYZ.Domain.Enums;

namespace XYZ.Domain.Entities
{
    public class Document : TenantScopedEntity
    {
        public string Name { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string? Description { get; set; }

        public DateTime UploadDate { get; set; } = DateTime.UtcNow;
        public string UploadedBy { get; set; } = string.Empty;

        public int DocumentDefinitionId { get; set; }
        public DocumentDefinition DocumentDefinition { get; set; } = null!;

        public int? StudentId { get; set; }
        public int? CoachId { get; set; }
        public int? AdminId { get; set; }

        public Student? Student { get; set; }
        public Coach? Coach { get; set; }
        public Admin? Admin { get; set; }
    }
}
