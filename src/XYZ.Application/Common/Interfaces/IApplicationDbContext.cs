using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Domain.Entities;

namespace XYZ.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Tenant> Tenants { get; }
        DbSet<ApplicationUser> Users { get; }
        DbSet<Student> Students { get; }
        DbSet<Coach> Coaches { get; }
        DbSet<Class> Classes { get; }
        DbSet<Attendance> Attendances { get; }
        DbSet<Payment> Payments { get; }
        DbSet<Document> Documents { get; }
        DbSet<ProgressRecord> ProgressRecords { get; }
        DbSet<Announcement> Announcements { get; }
        DbSet<Admin> Admins { get; }
        DbSet<Branch> Branches { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
