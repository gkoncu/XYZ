using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Data;
using XYZ.Application.Features.Tenants.Commands.CreateTenant;
using XYZ.Domain.Entities;

namespace XYZ.Application.Tests.Features.Tenants
{
    public class CreateTenantCommandHandlerTests
    {
        private static ApplicationDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Handle_WithValidRequest_CreatesTenant()
        {
            var context = CreateInMemoryContext();
            var handler = new CreateTenantCommandHandler(context);

            var command = new CreateTenantCommand
            {
                Name = "Test Kulübü",
                Subdomain = "testkulubu",
                Email = "info@testkulubu.local"
            };

            var id = await handler.Handle(command, CancellationToken.None);

            var tenant = await context.Tenants.FirstOrDefaultAsync(t => t.Id == id);

            Assert.NotNull(tenant);
            Assert.Equal("Test Kulübü", tenant!.Name);
            Assert.Equal("testkulubu", tenant.Subdomain);
            Assert.Equal("info@testkulubu.local", tenant.Email);

            Assert.True(tenant.IsActive);
            Assert.NotEqual(default, tenant.CreatedAt);
            Assert.Equal("Basic", tenant.SubscriptionPlan);
            Assert.True(tenant.SubscriptionEndDate > tenant.SubscriptionStartDate);
        }
    }
}
