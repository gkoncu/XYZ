using System.Collections.Generic;
using XYZ.Domain.Common;

namespace XYZ.Domain.Entities
{
    public class ProgressRecord : TenantScopedEntity
    {
        public int StudentId { get; set; }
        public int BranchId { get; set; }

        public DateOnly RecordDate { get; set; }

        public int Sequence { get; set; } = 1;

        public string? CreatedByDisplayName { get; set; }

        public string? CoachNotes { get; set; }
        public string? Goals { get; set; }

        public Student Student { get; set; } = null!;
        public Branch Branch { get; set; } = null!;

        public ICollection<ProgressRecordValue> Values { get; set; } = new List<ProgressRecordValue>();
    }
}
