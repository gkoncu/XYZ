using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Domain.Enums;

namespace XYZ.Application.Common.Interfaces
{
    public interface IPermissionService
    {
        Task<bool> HasPermissionAsync(string permissionKey, PermissionScope? minScope = null, CancellationToken ct = default);
        Task<PermissionScope?> GetScopeAsync(string permissionKey, CancellationToken ct = default);
    }
}
