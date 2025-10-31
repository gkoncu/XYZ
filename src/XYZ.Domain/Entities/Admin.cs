﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Domain.Common;

namespace XYZ.Domain.Entities
{
    public class Admin : BaseEntity
    {
        public string UserId { get; set; } = null!;
        public int TenantId { get; set; }
        public string? Department { get; set; }
        public string? Responsibilities { get; set; }
        public bool CanManageUsers { get; set; } = true;
        public bool CanManageFinance { get; set; } = true;
        public bool CanManageSettings { get; set; } = true;

        public ApplicationUser User { get; set; } = null!;
        public Tenant Tenant { get; set; } = null!;
    }
}
