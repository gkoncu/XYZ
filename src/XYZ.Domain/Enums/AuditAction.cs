using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Domain.Enums
{
    public enum AuditAction : byte
    {
        Create = 1,
        Update = 2,
        Delete = 3,
        SoftDelete = 4
    }
}
