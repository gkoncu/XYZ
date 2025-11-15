using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using XYZ.Application.Data;
using XYZ.Domain.Entities;

namespace XYZ.API.Dev;

internal static class DevIdentitySeeder
{
    private static readonly string[] Roles = { "Superadmin", "Admin", "Coach", "Student" };

    private sealed record UserSeed(string Email, string Password, string Role);
    private static readonly UserSeed[] Users =
    {
        new("superadmin@xyz.local", "Superadmin#123", "Superadmin"),
        new("admin@xyz.local",   "Admin#123",   "Admin"),
        new("coach@xyz.local",   "Coach#123",   "Coach"),
        new("student@xyz.local", "Student#123", "Student"),
    };

    public static async Task RunAsync(IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync(ct);

        var tenant = await db.Set<Tenant>().FirstOrDefaultAsync(t => t.Subdomain == "dev", ct);
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
                _ = await roleManager.CreateAsync(new IdentityRole(role));
        }

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        foreach (var seed in Users)
        {
            var user = await userManager.Users.FirstOrDefaultAsync(u => u.Email == seed.Email, ct);
            if (user is null)
            {
                user = new ApplicationUser
                {
                    UserName = seed.Email,
                    Email = seed.Email,
                    EmailConfirmed = true,
                    TenantId = tenant.Id
                };

                var create = await userManager.CreateAsync(user, seed.Password);
                if (!create.Succeeded) continue;

                if (!await userManager.IsInRoleAsync(user, seed.Role))
                    _ = await userManager.AddToRoleAsync(user, seed.Role);
            }
            else
            {
                if (user.GetType().GetProperty("TenantId")?.GetValue(user) is null)
                {
                    user.GetType().GetProperty("TenantId")?.SetValue(user, tenant.Id);
                    await userManager.UpdateAsync(user);
                }

                if (!await userManager.IsInRoleAsync(user, seed.Role))
                    _ = await userManager.AddToRoleAsync(user, seed.Role);
            }
        }
    }
}
