using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using XYZ.Application.Data;
using XYZ.Domain.Entities;

namespace XYZ.API.Dev;

internal static class DevIdentitySeeder
{
    private const string SuperAdminRoleName = "SuperAdmin";
    private const string SuperAdminEmail = "superadmin@xyz.local";
    private const string SuperAdminPassword = "Superadmin#123";

    private const string DevTenantSubdomain = "dev";
    private const string DevTenantName = "Dev Tenant";

    public static async Task RunAsync(IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync(ct);

        var tenant = await db.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Subdomain == DevTenantSubdomain, ct);

        if (tenant is null)
        {
            tenant = new Tenant
            {
                Name = DevTenantName,
                Subdomain = DevTenantSubdomain,
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

            db.Tenants.Add(tenant);
            await db.SaveChangesAsync(ct);
        }

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        if (!await roleManager.RoleExistsAsync(SuperAdminRoleName))
        {
            var roleCreate = await roleManager.CreateAsync(new IdentityRole(SuperAdminRoleName));
            if (!roleCreate.Succeeded)
            {
                var msg = string.Join(" | ", roleCreate.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"SuperAdmin rolü oluşturulamadı: {msg}");
            }
        }

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var user = await userManager.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == SuperAdminEmail, ct);

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = SuperAdminEmail,
                Email = SuperAdminEmail,
                EmailConfirmed = true,

                TenantId = tenant.Id,
                FirstName = "Super",
                LastName = "Admin",

                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var create = await userManager.CreateAsync(user, SuperAdminPassword);
            if (!create.Succeeded)
            {
                var msg = string.Join(" | ", create.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"SuperAdmin kullanıcı oluşturulamadı: {msg}");
            }
        }
        else
        {
            var changed = false;

            if (user.TenantId == 0)
            {
                user.TenantId = tenant.Id;
                changed = true;
            }

            if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                changed = true;
            }

            if (!user.IsActive)
            {
                user.IsActive = true;
                changed = true;
            }

            if (string.IsNullOrWhiteSpace(user.FirstName))
            {
                user.FirstName = "Super";
                changed = true;
            }

            if (string.IsNullOrWhiteSpace(user.LastName))
            {
                user.LastName = "Admin";
                changed = true;
            }

            if (changed)
            {
                user.UpdatedAt = DateTime.UtcNow;
                var update = await userManager.UpdateAsync(user);
                if (!update.Succeeded)
                {
                    var msg = string.Join(" | ", update.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"SuperAdmin kullanıcı güncellenemedi: {msg}");
                }
            }
        }

        if (!await userManager.IsInRoleAsync(user, SuperAdminRoleName))
        {
            var addRole = await userManager.AddToRoleAsync(user, SuperAdminRoleName);
            if (!addRole.Succeeded)
            {
                var msg = string.Join(" | ", addRole.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"SuperAdmin role eklenemedi: {msg}");
            }
        }
    }
}
