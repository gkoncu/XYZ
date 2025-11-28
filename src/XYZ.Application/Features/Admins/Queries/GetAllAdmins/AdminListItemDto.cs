using System;

namespace XYZ.Application.Features.Admins.Queries.GetAllAdmins
{
    public sealed class AdminListItemDto
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public int TenantId { get; set; }

        public string TenantName { get; set; } = string.Empty;

        public string IdentityNumber { get; set; } = string.Empty;

        public bool CanManageUsers { get; set; }

        public bool CanManageFinance { get; set; }

        public bool CanManageSettings { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
