using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Common.Interfaces
{
    public interface ICurrentUserService
    {
        string? UserId { get; }
        string? Role { get; }
        int? TenantId { get; }
        int? CoachId { get; }
        int? StudentId { get; }
        bool IsAuthenticated { get; }
    }
}
