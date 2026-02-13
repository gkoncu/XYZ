using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Domain.Enums;

namespace XYZ.Application.Common.Interfaces
{
    public interface IRequirePermission
    {
        string PermissionKey { get; }
        PermissionScope? MinimumScope { get; }
    }
}
