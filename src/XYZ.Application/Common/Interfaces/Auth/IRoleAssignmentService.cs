using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Domain.Entities;

namespace XYZ.Application.Common.Interfaces.Auth
{
    public interface IRoleAssignmentService
    {
        Task AssignRoleAsync(ApplicationUser user, string role, CancellationToken ct = default);
    }
}
