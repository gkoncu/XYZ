using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Domain.Common;

namespace XYZ.Application.Data
{
    internal static class ModelBuilderExtensions
    {
        /// <summary>
        /// TenantScopedEntity için ortak audit kolonlarının (CreatedByUserId / UpdatedByUserId) default ayarları.
        /// </summary>
        public static void ConfigureTenantAuditFields(this ModelBuilder builder)
        {
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                var clrType = entityType.ClrType;
                if (clrType == null) continue;

                if (!typeof(TenantScopedEntity).IsAssignableFrom(clrType))
                    continue;

                var createdProp = entityType.FindProperty(nameof(TenantScopedEntity.CreatedByUserId));
                if (createdProp is not null)
                    createdProp.SetMaxLength(64);

                var updatedProp = entityType.FindProperty(nameof(TenantScopedEntity.UpdatedByUserId));
                if (updatedProp is not null)
                    updatedProp.SetMaxLength(64);
            }
        }
    }
}
