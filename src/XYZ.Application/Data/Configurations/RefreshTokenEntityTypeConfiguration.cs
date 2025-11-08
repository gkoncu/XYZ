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
    public sealed class RefreshTokenEntityTypeConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> b)
        {
            b.ToTable("RefreshTokens", schema: "Auth");

            b.HasKey(x => x.Id);

            b.Property(x => x.UserId)
             .IsRequired()
             .HasMaxLength(128);

            b.Property(x => x.Hash)
             .IsRequired()
             .HasMaxLength(128);

            b.Property(x => x.TenantId);

            b.Property(x => x.CreatedAtUtc).IsRequired();
            b.Property(x => x.ExpiresAtUtc).IsRequired();
            b.Property(x => x.RevokedAtUtc);

            b.Property(x => x.ReplacedByTokenId);

            b.Property(x => x.CreatedByIp)
             .HasMaxLength(45);

            b.Property(x => x.UserAgent)
             .HasMaxLength(256);

            b.HasIndex(x => x.UserId);
            b.HasIndex(x => x.Hash).IsUnique();
        }
    }
}

