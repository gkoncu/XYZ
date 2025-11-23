using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using XYZ.Application.Data;
using XYZ.Domain.Entities;

namespace XYZ.API.Dev;

internal static class DevIdentitySeeder
{
    private static readonly string[] Roles = { "SuperAdmin", "Admin", "Coach", "Student" };

    private sealed record UserSeed(string Email, string Password, string Role);
    private static readonly UserSeed[] Users =
    {
        new("superadmin@xyz.local", "Superadmin#123", "SuperAdmin"),
        new("admin@xyz.local",   "Admin#123",   "Admin"),
        new("coach@xyz.local",   "Coach#123",   "Coach"),
        new("student@xyz.local", "Student#123", "Student"),
    };

    public static async Task RunAsync(IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync(ct);

        var tenant = await db.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Subdomain == "dev", ct);

        if (tenant is null)
        {
            tenant = new Tenant
            {
                Name = "Dev Tenant",
                Subdomain = "dev",
                Address = "N/A",
                Phone = null,
                Email = "dev@xyz.local",
                LogoUrl = null,
                PrimaryColor = "#3B82F6",
                SecondaryColor = "#1E40AF",
                SubscriptionStartDate = DateTime.UtcNow.Date,
                SubscriptionEndDate = DateTime.UtcNow.Date.AddYears(1),
                SubscriptionPlan = "Basic",
                IsActive = true
            };
            db.Add(tenant);
            await db.SaveChangesAsync(ct);
        }

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                _ = await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        foreach (var seed in Users)
        {
            var user = await userManager.Users
                .FirstOrDefaultAsync(u => u.Email == seed.Email, ct);

            if (user is null)
            {
                var firstName = seed.Role switch
                {
                    "SuperAdmin" => "Super",
                    "Admin" => "Dev",
                    "Coach" => "Demo",
                    "Student" => "Demo",
                    _ => "Dev"
                };

                var lastName = seed.Role switch
                {
                    "SuperAdmin" => "Admin",
                    "Admin" => "Admin",
                    "Coach" => "Coach",
                    "Student" => "Student",
                    _ => "User"
                };

                user = new ApplicationUser
                {
                    UserName = seed.Email,
                    Email = seed.Email,
                    EmailConfirmed = true,
                    TenantId = tenant.Id,
                    FirstName = firstName,
                    LastName = lastName,
                    IsActive = true
                };

                var create = await userManager.CreateAsync(user, seed.Password);
                if (!create.Succeeded) continue;

                if (!await userManager.IsInRoleAsync(user, seed.Role))
                {
                    _ = await userManager.AddToRoleAsync(user, seed.Role);
                }
            }
            else
            {
                var tenantIdProp = user.GetType().GetProperty("TenantId");
                if (tenantIdProp?.GetValue(user) is null or 0)
                {
                    tenantIdProp?.SetValue(user, tenant.Id);
                    await userManager.UpdateAsync(user);
                }

                if (!await userManager.IsInRoleAsync(user, seed.Role))
                {
                    _ = await userManager.AddToRoleAsync(user, seed.Role);
                }
            }
        }

        var branch = await db.Branches
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(b => b.TenantId == tenant.Id, ct);

        if (branch is null)
        {
            branch = new Branch
            {
                Name = "Merkez Şube",
                TenantId = tenant.Id,
                IsActive = true
            };

            db.Branches.Add(branch);
            await db.SaveChangesAsync(ct);
        }

        var demoClass = await db.Classes
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.TenantId == tenant.Id, ct);

        if (demoClass is null)
        {
            demoClass = new Class
            {
                Name = "Demo Basketbol U12",
                Description = "Geliştirme ortamı için örnek sınıf.",
                TenantId = tenant.Id,
                BranchId = branch.Id,
                MaxCapacity = 20,
                AgeGroupMin = 10,
                AgeGroupMax = 13,
                IsActive = true
            };

            db.Classes.Add(demoClass);
            await db.SaveChangesAsync(ct);
        }

        var superAdminUser = await userManager.FindByEmailAsync("superadmin@xyz.local");
        var adminUser = await userManager.FindByEmailAsync("admin@xyz.local");
        var coachUser = await userManager.FindByEmailAsync("coach@xyz.local");
        var studentUser = await userManager.FindByEmailAsync("student@xyz.local");

        if (adminUser is not null)
        {
            var adminProfile = await db.Admins
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(a => a.UserId == adminUser.Id, ct);

            if (adminProfile is null)
            {
                adminProfile = new Admin
                {
                    UserId = adminUser.Id,
                    TenantId = tenant.Id,
                    IdentityNumber = "ADMIN-DEV-001",
                    CanManageUsers = true,
                    CanManageFinance = true,
                    CanManageSettings = true,
                    IsActive = true
                };
                db.Admins.Add(adminProfile);
            }
        }

        Coach? coachProfile = null;
        if (coachUser is not null)
        {
            coachProfile = await db.Coaches
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.UserId == coachUser.Id, ct);

            if (coachProfile is null)
            {
                coachProfile = new Coach
                {
                    UserId = coachUser.Id,
                    TenantId = tenant.Id,
                    IdentityNumber = "COACH-DEV-001",
                    LicenseNumber = "LIC-DEV-001",
                    BranchId = branch.Id,
                    IsActive = true
                };
                db.Coaches.Add(coachProfile);
            }
            else
            {
                if (coachProfile.BranchId == 0)
                {
                    coachProfile.BranchId = branch.Id;
                }

                if (!coachProfile.IsActive)
                {
                    coachProfile.IsActive = true;
                }
            }
        }

        Student? studentProfile = null;
        if (studentUser is not null)
        {
            studentProfile = await db.Students
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.UserId == studentUser.Id, ct);

            if (studentProfile is null)
            {
                studentProfile = new Student
                {
                    UserId = studentUser.Id,
                    TenantId = tenant.Id,
                    ClassId = demoClass.Id,
                    IdentityNumber = "STUDENT-DEV-001",
                    Address = "N/A",
                    Parent1FirstName = "Demo",
                    Parent1LastName = "Veli",
                    Parent1PhoneNumber = "+90 555 000 0000",
                    IsActive = true
                };
                db.Students.Add(studentProfile);
            }
            else
            {
                if (studentProfile.ClassId is null)
                {
                    studentProfile.ClassId = demoClass.Id;
                }

                if (!studentProfile.IsActive)
                {
                    studentProfile.IsActive = true;
                }
            }
        }

        await db.SaveChangesAsync(ct);

        if (coachProfile is not null)
        {
            var cls = await db.Classes
                .Include(c => c.Coaches)
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == demoClass.Id, ct);

            if (cls is not null && !cls.Coaches.Any(c => c.Id == coachProfile.Id))
            {
                cls.Coaches.Add(coachProfile);
                await db.SaveChangesAsync(ct);
            }
        }

        if (studentUser is not null)
        {
            studentProfile = await db.Students
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.UserId == studentUser.Id, ct);
        }

        if (studentProfile is not null)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

            var existingEnrollment = await db.ClassEnrollments
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(e =>
                    e.StudentId == studentProfile.Id &&
                    e.ClassId == demoClass.Id &&
                    e.EndDate == null,
                    ct);

            if (existingEnrollment is null)
            {
                var enrollment = new ClassEnrollment
                {
                    StudentId = studentProfile.Id,
                    ClassId = demoClass.Id,
                    StartDate = today,
                    EndDate = null,
                    IsActive = true
                };

                db.ClassEnrollments.Add(enrollment);
                await db.SaveChangesAsync(ct);
            }
        }
    }
}
