using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using XYZ.Domain.Common;

namespace XYZ.Domain.Entities
{
    public class Document : BaseEntity
    {
        public int StudentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DocumentType Type { get; set; }
        public DateTime UploadDate { get; set; } = DateTime.UtcNow;
        public string UploadedBy { get; set; } = string.Empty;

        public Student Student { get; set; } = null!;
    }
}
