using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        public string? Role => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);

        public string? Branch => _httpContextAccessor.HttpContext?.User?.FindFirstValue("Branch");

        public int? TenantId => GetClaimValueAsInt("TenantId");

        public int? CoachId => GetClaimValueAsInt("CoachId");

        public int? StudentId => GetClaimValueAsInt("StudentId");

        public bool IsAuthenticated => UserId != null;

        private int? GetClaimValueAsInt(string claimType)
        {
            var value = _httpContextAccessor.HttpContext?.User?.FindFirstValue(claimType);
            return int.TryParse(value, out var result) ? result : null;
        }
    }
}
