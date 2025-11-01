using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
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
        public DbSet<ClassAssistantCoach> ClassAssistantCoaches { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Domain.Entities.Document> Documents { get; set; }
        public DbSet<ProgressRecord> ProgressRecords { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Announcement> Announcements { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>().HasQueryFilter(u => u.IsActive);

            // Soft Delete Query Filters
            builder.Entity<Tenant>().HasQueryFilter(t => t.IsActive);
            builder.Entity<Student>().HasQueryFilter(s => s.IsActive);
            builder.Entity<Coach>().HasQueryFilter(c => c.IsActive);
            builder.Entity<Admin>().HasQueryFilter(a => a.IsActive);
            builder.Entity<Class>().HasQueryFilter(c => c.IsActive);
            builder.Entity<ClassSchedule>().HasQueryFilter(cs => cs.IsActive);
            builder.Entity<ClassAssistantCoach>().HasQueryFilter(cac => cac.IsActive);
            builder.Entity<Attendance>().HasQueryFilter(a => a.IsActive);
            builder.Entity<Domain.Entities.Document>().HasQueryFilter(d => d.IsActive);
            builder.Entity<ProgressRecord>().HasQueryFilter(pr => pr.IsActive);
            builder.Entity<Payment>().HasQueryFilter(p => p.IsActive);
            builder.Entity<Announcement>().HasQueryFilter(a => a.IsActive);

            ConfigureDecimalPrecisions(builder);

            ConfigureOptionalRelationships(builder);

            // TODO : Tenant-based Query Filters (Multi-tenancy)
            // builder.Entity<Student>().HasQueryFilter(s => s.TenantId == _tenantService.GetCurrentTenantId());
            // builder.Entity<Coach>().HasQueryFilter(c => c.TenantId == _tenantService.GetCurrentTenantId());
        }

        private void ConfigureDecimalPrecisions(ModelBuilder builder)
        {
            builder.Entity<Payment>(entity =>
            {
                entity.Property(p => p.Amount).HasPrecision(18, 2);
                entity.Property(p => p.DiscountAmount).HasPrecision(18, 2);
            });

            builder.Entity<ProgressRecord>(entity =>
            {
                entity.Property(p => p.Height).HasPrecision(5, 2);
                entity.Property(p => p.Weight).HasPrecision(5, 2);
                entity.Property(p => p.BodyFatPercentage).HasPrecision(4, 1);
                entity.Property(p => p.VerticalJump).HasPrecision(4, 1);
                entity.Property(p => p.SprintTime).HasPrecision(4, 2);
                entity.Property(p => p.Endurance).HasPrecision(4, 1);
                entity.Property(p => p.Flexibility).HasPrecision(4, 1);
            });

            builder.Entity<Student>(entity =>
            {
                entity.Property(s => s.MonthlyFee).HasPrecision(18, 2);
            });
        }

        private void ConfigureOptionalRelationships(ModelBuilder builder)
        {
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.HasOne(au => au.Tenant)
                      .WithMany(t => t.Users)
                      .HasForeignKey(au => au.TenantId)
                      .IsRequired(false);
            });

            builder.Entity<ClassAssistantCoach>(entity =>
            {
                entity.HasOne(cac => cac.Class)
                      .WithMany(c => c.AssistantCoaches)
                      .HasForeignKey(cac => cac.ClassId)
                      .IsRequired(false);

                entity.HasOne(cac => cac.Coach)
                      .WithMany(c => c.AssistantClasses)
                      .HasForeignKey(cac => cac.CoachId)
                      .IsRequired(false);
            });

            builder.Entity<Domain.Entities.Document>(entity =>
            {
                entity.HasOne(d => d.Student)
                      .WithMany(s => s.Documents)
                      .HasForeignKey(d => d.StudentId)
                      .IsRequired(false);

                entity.HasOne(d => d.Coach)
                      .WithMany(c => c.Documents)
                      .HasForeignKey(d => d.CoachId)
                      .IsRequired(false);

                entity.HasOne(d => d.Admin)
                      .WithMany(a => a.Documents)
                      .HasForeignKey(d => d.AdminId)
                      .IsRequired(false);
            });

            builder.Entity<Student>(entity =>
            {
                entity.HasOne(s => s.Class)
                      .WithMany(c => c.Students)
                      .HasForeignKey(s => s.ClassId)
                      .IsRequired(false);
            });

            builder.Entity<Attendance>(entity =>
            {
                entity.HasOne(a => a.Student)
                      .WithMany(s => s.Attendances)
                      .HasForeignKey(a => a.StudentId)
                      .IsRequired(false);

                entity.HasOne(a => a.ClassSchedule)
                      .WithMany(cs => cs.Attendances)
                      .HasForeignKey(a => a.ClassScheduleId)
                      .IsRequired(false);
            });
        }
    }
}