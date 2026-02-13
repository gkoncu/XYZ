using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Domain.Common;
using XYZ.Domain.Enums;

namespace XYZ.Domain.Entities
{
    public sealed class TenantUserPermissionOverride : TenantScopedEntity
    {
        public string UserId { get; set; } = null!;
        public string PermissionKey { get; set; } = null!;
        public PermissionScope Scope { get; set; } = PermissionScope.Self;
    }
}
