using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Entities;

namespace XYZ.Application.Data
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
        public DbSet<Attendance> Attendances => Set<Attendance>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<PaymentPlan> PaymentPlans => Set<PaymentPlan>();
        public DbSet<Document> Documents => Set<Document>();
        public DbSet<ProgressRecord> ProgressRecords => Set<ProgressRecord>();
        public DbSet<Announcement> Announcements => Set<Announcement>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<Admin> Admins => Set<Admin>();
        public DbSet<Branch> Branches => Set<Branch>();
        public DbSet<ClassSession> ClassSessions => Set<ClassSession>();
        public DbSet<ClassEnrollment> ClassEnrollments => Set<ClassEnrollment>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            builder.Entity<ApplicationUser>().HasQueryFilter(u => u.IsActive);
            builder.Entity<Tenant>().HasQueryFilter(t => t.IsActive);
            builder.Entity<Student>().HasQueryFilter(s => s.IsActive);
            builder.Entity<Coach>().HasQueryFilter(c => c.IsActive);
            builder.Entity<Admin>().HasQueryFilter(a => a.IsActive);
            builder.Entity<Class>().HasQueryFilter(c => c.IsActive);
            builder.Entity<ClassSession>().HasQueryFilter(cs => cs.IsActive);
            builder.Entity<ClassEnrollment>().HasQueryFilter(ce => ce.IsActive);
            builder.Entity<Attendance>().HasQueryFilter(a => a.IsActive);
            builder.Entity<Document>().HasQueryFilter(d => d.IsActive);
            builder.Entity<ProgressRecord>().HasQueryFilter(pr => pr.IsActive);
            builder.Entity<Payment>().HasQueryFilter(p => p.IsActive);
            builder.Entity<Announcement>().HasQueryFilter(a => a.IsActive);
            builder.Entity<Branch>().HasQueryFilter(b => b.IsActive);
            builder.Entity<PaymentPlan>().HasQueryFilter(pp => pp.IsActive);

            ConfigureCascadeRestrictions(builder);
            ConfigureDecimalPrecisions(builder);
            ConfigureOptionalRelationships(builder);

            // TODO : Tenant-based Query Filters (Multi-tenancy)
            // builder.Entity<Student>().HasQueryFilter(s => s.TenantId == _tenantService.GetCurrentTenantId());
            // builder.Entity<Coach>().HasQueryFilter(c => c.TenantId == _tenantService.GetCurrentTenantId());
            // builder.Entity<Class>().HasQueryFilter(c => c.TenantId == _tenantService.GetCurrentTenantId());
        }

        private void ConfigureCascadeRestrictions(ModelBuilder builder)
        {
            builder.Entity<Class>(entity =>
            {
                entity.HasOne(c => c.Tenant)
                      .WithMany(t => t.Classes)
                      .HasForeignKey(c => c.TenantId)
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

            builder.Entity<ClassSession>(entity =>
            {
                entity.HasOne(cs => cs.Class)
                      .WithMany()
                      .HasForeignKey(cs => cs.ClassId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(cs => cs.Title)
                      .HasMaxLength(200)
                      .IsRequired();

                entity.Property(cs => cs.Location)
                      .HasMaxLength(200);

                entity.Property(cs => cs.Description)
                      .HasMaxLength(2000);

                entity.Property(cs => cs.CoachNote)
                      .HasMaxLength(2000);

                entity.HasIndex(cs => new { cs.ClassId, cs.Date });
            });

            builder.Entity<ClassEnrollment>(entity =>
            {
                entity.HasOne(e => e.Student)
                      .WithMany()
                      .HasForeignKey(e => e.StudentId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Class)
                      .WithMany()
                      .HasForeignKey(e => e.ClassId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.ClassId, e.StudentId, e.StartDate });
            });

            builder.Entity<Attendance>(entity =>
            {
                entity.HasOne(a => a.Student)
                      .WithMany(s => s.Attendances)
                      .HasForeignKey(a => a.StudentId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.ClassSession)
                      .WithMany(cs => cs.Attendances)
                      .HasForeignKey(a => a.ClassSessionId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.Class)
                      .WithMany()
                      .HasForeignKey(a => a.ClassId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(a => a.Note)
                      .HasMaxLength(1000);

                entity.Property(a => a.CoachComment)
                      .HasMaxLength(2000);

                entity.HasIndex(a => new { a.ClassSessionId, a.StudentId })
                      .IsUnique();
            });

            builder.Entity<ProgressRecord>(entity =>
            {
                entity.HasOne(pr => pr.Student)
                      .WithMany(s => s.ProgressRecords)
                      .HasForeignKey(pr => pr.StudentId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Document>(entity =>
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

            builder.Entity<PaymentPlan>(entity =>
            {
                entity.Property(pp => pp.TotalAmount).HasPrecision(18, 2);
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
        }

        private void ConfigureOptionalRelationships(ModelBuilder builder)
        {
            builder.Entity<Class>()
                    .HasMany(c => c.Coaches)
                    .WithMany(co => co.Classes)
                    .UsingEntity<Dictionary<string, object>>(
                        "ClassCoach",
                        j => j.HasOne<Coach>()
                              .WithMany()
                              .HasForeignKey("CoachId")
                              .OnDelete(DeleteBehavior.Restrict),
                        j => j.HasOne<Class>()
                              .WithMany()
                              .HasForeignKey("ClassId")
                              .OnDelete(DeleteBehavior.Restrict),
                        j =>
                        {
                            j.HasKey("ClassId", "CoachId");
                        });

            builder.Entity<ApplicationUser>(entity =>
            {
                entity.HasOne(au => au.Tenant)
                      .WithMany(t => t.Users)
                      .HasForeignKey(au => au.TenantId)
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

            builder.Entity<PaymentPlan>(entity =>
            {
                entity.HasOne(pp => pp.Student)
                      .WithMany(s => s.PaymentPlans)
                      .HasForeignKey(pp => pp.StudentId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(pp => pp.Tenant)
                      .WithMany(t => t.PaymentPlans)
                      .HasForeignKey(pp => pp.TenantId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Payment>(entity =>
            {
                entity.HasOne(p => p.PaymentPlan)
                      .WithMany(pp => pp.Payments)
                      .HasForeignKey(p => p.PaymentPlanId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .IsRequired(false);
            });


        }
    }
}
