using FluentValidation;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Data;
using XYZ.Application.Features.Auth.Login.Commands;
using XYZ.Application.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            var config = TypeAdapterConfig.GlobalSettings;

            var mappingConfigs = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface && typeof(IMapsterConfig).IsAssignableFrom(t));

            foreach (var mappingConfigType in mappingConfigs)
            {
                if (Activator.CreateInstance(mappingConfigType) is IMapsterConfig instance)
                {
                    instance.Configure(config);
                }
            }

            services.AddSingleton(config);
            services.AddScoped<IMapper, ServiceMapper>();

            //services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            //services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            var authAssembly = typeof(LoginCommand).Assembly;

            services.AddValidatorsFromAssembly(authAssembly);
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(authAssembly));

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(XYZ.Application.Common.Behaviors.ValidationBehavior<,>));


            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IApplicationDbContext>(sp =>
                sp.GetRequiredService<ApplicationDbContext>());

            services.AddScoped<IDataScopeService, DataScopeService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<ITenantService, TenantService>();
            services.AddScoped<IExcelExporter, ExcelExporter>();
            services.AddScoped<IOverduePaymentService, OverduePaymentService>();

            return services;
        }
    }
}
