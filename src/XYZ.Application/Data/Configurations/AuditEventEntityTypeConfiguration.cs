using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Domain.Entities;

namespace XYZ.Application.Data.Configurations
{
    public sealed class AuditEventEntityTypeConfiguration : IEntityTypeConfiguration<AuditEvent>
    {
        public void Configure(EntityTypeBuilder<AuditEvent> b)
        {
            b.HasKey(x => x.Id);

            b.Property(x => x.EntityName)
                .HasMaxLength(128)
                .IsRequired();

            b.Property(x => x.EntityKey)
                .HasMaxLength(64)
                .IsRequired();

            b.Property(x => x.ActorUserId)
                .HasMaxLength(64);

            b.Property(x => x.Action)
                .HasConversion<byte>();

            b.Property(x => x.OccurredAtUtc)
                .IsRequired();

            b.Property(x => x.ChangesJson)
                .HasColumnType("nvarchar(max)");

            b.HasIndex(x => new { x.TenantId, x.OccurredAtUtc });
            b.HasIndex(x => new { x.TenantId, x.EntityName, x.EntityKey });
            b.HasIndex(x => new { x.TenantId, x.ActorUserId, x.OccurredAtUtc });

            b.HasOne<Tenant>()
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
