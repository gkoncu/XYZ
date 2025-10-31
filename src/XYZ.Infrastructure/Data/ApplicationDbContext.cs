using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using System.Security.Claims;
using XYZ.Domain.Entities;

namespace XYZ.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Coach> Coaches { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<ClassSchedule> ClassSchedules { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Domain.Entities.Document> Documents { get; set; }
        public DbSet<ProgressRecord> ProgressRecords { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Announcement> Announcements { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Soft Delete Query Filters
            builder.Entity<Tenant>().HasQueryFilter(t => t.IsActive);
            builder.Entity<Student>().HasQueryFilter(s => s.IsActive);
            builder.Entity<Coach>().HasQueryFilter(c => c.IsActive);
            builder.Entity<Admin>().HasQueryFilter(a => a.IsActive);
            builder.Entity<Class>().HasQueryFilter(c => c.IsActive);
            builder.Entity<ClassSchedule>().HasQueryFilter(cs => cs.IsActive);
            builder.Entity<Attendance>().HasQueryFilter(a => a.IsActive);
            builder.Entity<Domain.Entities.Document>().HasQueryFilter(d => d.IsActive);
            builder.Entity<ProgressRecord>().HasQueryFilter(pr => pr.IsActive);
            builder.Entity<Payment>().HasQueryFilter(p => p.IsActive);
            builder.Entity<Announcement>().HasQueryFilter(a => a.IsActive);

            //// Tenant-based Query Filters (Multi-tenancy)
            //builder.Entity<Student>().HasQueryFilter(s => s.TenantId == _tenantService.GetCurrentTenantId());
            //builder.Entity<Coach>().HasQueryFilter(c => c.TenantId == _tenantService.GetCurrentTenantId());
        }
    }
}