using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Branches.Queries.GetBranchById
{
    public class BranchDetailDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int TenantId { get; set; }

        public string TenantName { get; set; } = string.Empty;

        public int ClassCount { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsActive { get; set; }
    }
}
