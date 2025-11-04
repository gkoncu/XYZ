using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Entities;

namespace XYZ.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Tenant> Tenants => Set<Tenant>();
        public DbSet<Student> Students => Set<Student>();
        public DbSet<Coach> Coaches => Set<Coach>();
        public DbSet<Class> Classes => Set<Class>();
        public DbSet<ClassAssistantCoach> ClassAssistantCoaches => Set<ClassAssistantCoach>();
        public DbSet<ClassSchedule> ClassSchedules => Set<ClassSchedule>();
        public DbSet<Attendance> Attendances => Set<Attendance>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<Document> Documents => Set<Document>();
        public DbSet<ProgressRecord> ProgressRecords => Set<ProgressRecord>();
        public DbSet<Announcement> Announcements => Set<Announcement>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

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

            ConfigureCascadeRestrictions(builder);

            ConfigureDecimalPrecisions(builder);

            ConfigureOptionalRelationships(builder);

            // TODO : Tenant-based Query Filters (Multi-tenancy)
            // builder.Entity<Student>().HasQueryFilter(s => s.TenantId == _tenantService.GetCurrentTenantId());
            // builder.Entity<Coach>().HasQueryFilter(c => c.TenantId == _tenantService.GetCurrentTenantId());
        }

        private void ConfigureCascadeRestrictions(ModelBuilder builder)
        {
            builder.Entity<Class>(entity =>
            {
                entity.HasOne(c => c.Tenant)
                      .WithMany(t => t.Classes)
                      .HasForeignKey(c => c.TenantId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.HeadCoach)
                      .WithMany(co => co.HeadClasses)
                      .HasForeignKey(c => c.HeadCoachId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Student>(entity =>
            {
                entity.HasOne(s => s.Tenant)
                      .WithMany(t => t.Students)
                      .HasForeignKey(s => s.TenantId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.User)
                      .WithOne(u => u.StudentProfile)
                      .HasForeignKey<Student>(s => s.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.Class)
                      .WithMany(c => c.Students)
                      .HasForeignKey(s => s.ClassId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Coach>(entity =>
            {
                entity.HasOne(c => c.Tenant)
                      .WithMany(t => t.Coaches)
                      .HasForeignKey(c => c.TenantId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.User)
                      .WithOne(u => u.CoachProfile)
                      .HasForeignKey<Coach>(c => c.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Admin>(entity =>
            {
                entity.HasOne(a => a.Tenant)
                      .WithMany(t => t.Admins)
                      .HasForeignKey(a => a.TenantId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.User)
                      .WithOne(u => u.AdminProfile)
                      .HasForeignKey<Admin>(a => a.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Payment>(entity =>
            {
                entity.HasOne(p => p.Tenant)
                      .WithMany(t => t.Payments)
                      .HasForeignKey(p => p.TenantId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Student)
                      .WithMany(s => s.Payments)
                      .HasForeignKey(p => p.StudentId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Announcement>(entity =>
            {
                entity.HasOne(a => a.Tenant)
                      .WithMany(t => t.Announcements)
                      .HasForeignKey(a => a.TenantId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<ClassSchedule>(entity =>
            {
                entity.HasOne(cs => cs.Class)
                      .WithMany(c => c.Schedules)
                      .HasForeignKey(cs => cs.ClassId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Attendance>(entity =>
            {
                entity.HasOne(a => a.Student)
                      .WithMany(s => s.Attendances)
                      .HasForeignKey(a => a.StudentId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.ClassSchedule)
                      .WithMany(cs => cs.Attendances)
                      .HasForeignKey(a => a.ClassScheduleId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<ProgressRecord>(entity =>
            {
                entity.HasOne(pr => pr.Student)
                      .WithMany(s => s.ProgressRecords)
                      .HasForeignKey(pr => pr.StudentId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<ClassAssistantCoach>(entity =>
            {
                entity.HasOne(cac => cac.Class)
                      .WithMany(c => c.AssistantCoaches)
                      .HasForeignKey(cac => cac.ClassId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(cac => cac.Coach)
                      .WithMany(c => c.AssistantClasses)
                      .HasForeignKey(cac => cac.CoachId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Domain.Entities.Document>(entity =>
            {
                entity.HasOne(d => d.Student)
                      .WithMany(s => s.Documents)
                      .HasForeignKey(d => d.StudentId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Coach)
                      .WithMany(c => c.Documents)
                      .HasForeignKey(d => d.CoachId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Admin)
                      .WithMany(a => a.Documents)
                      .HasForeignKey(d => d.AdminId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
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

            builder.Entity<Document>(entity =>
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