using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using XYZ.Domain.Common;

namespace XYZ.Domain.Entities
{
    public class Student : BaseEntity
    {
        public string UserId { get; set; } = null!;
        public int TenantId { get; set; }
        public int? ClassId { get; set; }
        public string IdentityNumber { get; set; } = string.Empty;
        public string? Address { get; set; }

        public string? Parent1FirstName { get; set; }
        public string? Parent1LastName { get; set; }
        public string? Parent1Email { get; set; }
        public string? Parent1PhoneNumber { get; set; }

        public string? Parent2FirstName { get; set; }
        public string? Parent2LastName { get; set; }
        public string? Parent2Email { get; set; }
        public string? Parent2PhoneNumber { get; set; }

        public string? MedicalInformation { get; set; }
        public string? Notes { get; set; }

        public ApplicationUser User { get; set; } = null!;
        public Tenant Tenant { get; set; } = null!;
        public Class? Class { get; set; }
        public ICollection<Document> Documents { get; set; } = new List<Document>();
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public ICollection<ProgressRecord> ProgressRecords { get; set; } = new List<ProgressRecord>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<PaymentPlan> PaymentPlans { get; set; } = new List<PaymentPlan>();
    }
}
