using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Common;
using XYZ.Domain.Entities;
using XYZ.Domain.Enums;

namespace XYZ.Application.Data.Interceptors
{
    /// <summary>
    /// Audit otomatik yazımı:
    /// - TenantScopedEntity: CreatedByUserId / UpdatedByUserId set eder.
    /// - Değişiklik geçmişi: AuditEvent üretir.
    ///
    /// Not: AuditEvent kendisi auditlenmez.
    /// </summary>
    public sealed class AuditSaveChangesInterceptor : SaveChangesInterceptor
    {
        private static readonly AsyncLocal<bool> _suppress = new();

        private readonly ICurrentUserService _current;
        private readonly List<PendingCreateAudit> _pendingCreates = new();

        private static readonly HashSet<string> _ignoredPropertyNames = new(StringComparer.Ordinal)
        {
            "PasswordHash",
            "SecurityStamp",
            "ConcurrencyStamp",
            "TwoFactorEnabled",
            "PhoneNumber",
            "PhoneNumberConfirmed",
            "Email",
            "EmailConfirmed",
            "NormalizedEmail",
            "NormalizedUserName",

            "Hash",
            "Token",
            "RefreshToken",
            "Secret",
            "ApiKey",

            nameof(BaseEntity.CreatedAt),
            nameof(BaseEntity.UpdatedAt),
            nameof(TenantScopedEntity.CreatedByUserId),
            nameof(TenantScopedEntity.UpdatedByUserId)
        };

        public AuditSaveChangesInterceptor(ICurrentUserService current)
        {
            _current = current;
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            if (eventData.Context is not DbContext ctx)
                return base.SavingChanges(eventData, result);

            if (_suppress.Value)
                return base.SavingChanges(eventData, result);

            PrepareAudit(ctx);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context is DbContext ctx && !_suppress.Value)
            {
                PrepareAudit(ctx);
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
        {
            if (eventData.Context is DbContext ctx && !_suppress.Value)
            {
                FlushPendingCreates(ctx, CancellationToken.None).GetAwaiter().GetResult();
            }

            return base.SavedChanges(eventData, result);
        }

        public override async ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context is DbContext ctx && !_suppress.Value)
            {
                await FlushPendingCreates(ctx, cancellationToken);
            }

            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        public override void SaveChangesFailed(DbContextErrorEventData eventData)
        {
            _pendingCreates.Clear();
            base.SaveChangesFailed(eventData);
        }

        public override Task SaveChangesFailedAsync(
            DbContextErrorEventData eventData,
            CancellationToken cancellationToken = default)
        {
            _pendingCreates.Clear();
            return base.SaveChangesFailedAsync(eventData, cancellationToken);
        }

        private void PrepareAudit(DbContext ctx)
        {
            var nowUtc = DateTime.UtcNow;
            var actorUserId = _current.UserId;

            foreach (var entry in ctx.ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Unchanged || entry.State == EntityState.Detached)
                    continue;

                if (entry.Entity is AuditEvent)
                    continue;

                if (entry.Entity is not TenantScopedEntity tenantEntity)
                    continue;

                if (tenantEntity.TenantId <= 0 && _current.TenantId.HasValue)
                    tenantEntity.TenantId = _current.TenantId.Value;

                switch (entry.State)
                {
                    case EntityState.Added:
                        if (!string.IsNullOrWhiteSpace(actorUserId) && string.IsNullOrWhiteSpace(tenantEntity.CreatedByUserId))
                            tenantEntity.CreatedByUserId = actorUserId;

                        _pendingCreates.Add(new PendingCreateAudit(entry, tenantEntity.TenantId, actorUserId));
                        break;

                    case EntityState.Modified:
                        tenantEntity.UpdatedAt = nowUtc;
                        if (!string.IsNullOrWhiteSpace(actorUserId))
                            tenantEntity.UpdatedByUserId = actorUserId;

                        ctx.Set<AuditEvent>().Add(BuildAuditEventForModified(entry, tenantEntity, nowUtc, actorUserId));
                        break;

                    case EntityState.Deleted:
                        ctx.Set<AuditEvent>().Add(BuildAuditEventForDeleted(entry, tenantEntity, nowUtc, actorUserId));
                        break;
                }
            }
        }

        private static AuditEvent BuildAuditEventForModified(EntityEntry entry, TenantScopedEntity e, DateTime nowUtc, string? actorUserId)
        {
            var changes = new Dictionary<string, object?>(StringComparer.Ordinal);

            foreach (var p in entry.Properties)
            {
                if (p.Metadata.IsPrimaryKey())
                    continue;

                if (!p.IsModified)
                    continue;

                var name = p.Metadata.Name;
                if (_ignoredPropertyNames.Contains(name))
                    continue;

                changes[name] = p.CurrentValue;
            }

            var action = AuditAction.Update;
            if (entry.Properties.Any(p => p.Metadata.Name == nameof(BaseEntity.IsActive)
                                       && p.IsModified
                                       && p.OriginalValue is bool o
                                       && o == true
                                       && p.CurrentValue is bool n
                                       && n == false))
            {
                action = AuditAction.SoftDelete;
            }

            return new AuditEvent
            {
                TenantId = e.TenantId,
                EntityName = entry.Metadata.ClrType.Name,
                EntityKey = GetEntityKey(entry),
                Action = action,
                ActorUserId = actorUserId,
                OccurredAtUtc = nowUtc,
                ChangesJson = changes.Count == 0 ? null : JsonSerializer.Serialize(changes)
            };
        }

        private static AuditEvent BuildAuditEventForDeleted(EntityEntry entry, TenantScopedEntity e, DateTime nowUtc, string? actorUserId)
        {
            return new AuditEvent
            {
                TenantId = e.TenantId,
                EntityName = entry.Metadata.ClrType.Name,
                EntityKey = GetEntityKey(entry),
                Action = AuditAction.Delete,
                ActorUserId = actorUserId,
                OccurredAtUtc = nowUtc,
                ChangesJson = null
            };
        }

        private static string GetEntityKey(EntityEntry entry)
        {
            var id = entry.Property("Id").CurrentValue;
            return id?.ToString() ?? string.Empty;
        }

        private async Task FlushPendingCreates(DbContext ctx, CancellationToken ct)
        {
            if (_pendingCreates.Count == 0)
                return;

            var nowUtc = DateTime.UtcNow;
            var events = new List<AuditEvent>(_pendingCreates.Count);

            foreach (var p in _pendingCreates)
            {
                var entityName = p.Entry.Metadata.ClrType.Name;
                var entityKey = GetEntityKey(p.Entry);

                events.Add(new AuditEvent
                {
                    TenantId = p.TenantId,
                    EntityName = entityName,
                    EntityKey = entityKey,
                    Action = AuditAction.Create,
                    ActorUserId = p.ActorUserId,
                    OccurredAtUtc = nowUtc,
                    ChangesJson = null
                });
            }

            _pendingCreates.Clear();

            _suppress.Value = true;
            try
            {
                ctx.Set<AuditEvent>().AddRange(events);
                await ctx.SaveChangesAsync(ct);
            }
            finally
            {
                _suppress.Value = false;
            }
        }

        private sealed record PendingCreateAudit(EntityEntry Entry, int TenantId, string? ActorUserId);
    }
}
