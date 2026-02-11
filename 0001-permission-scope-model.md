# ADR-0001: Permission + Scope (Tenant Role Template + User Override)

**Durum:** Proposed | Accepted | Superseded  
**Tarih:** YYYY-MM-DD

## Context
- SaaS modelinde tenant bazlý farklý ihtiyaçlar
- Ayný rolde kullanýcý bazlý farklý yetki/scope ihtiyacý
- Handler’larda rol string kontrolünü azaltma hedefi

## Decision
- Identity Role = default þablon
- TenantRolePermission + TenantUserPermissionOverride tablolarý
- EffectivePermissionSet çözümleme + cache/versioning
- Scope seviyeleri: Self/OwnClasses/Branch/Tenant/AllTenants

## Consequences
- Daha esnek yetki yönetimi
- UI’da rol þablonu + kullanýcý override ekranlarý gerekir
- Cache invalidation versioning gerektirir

## Alternatives considered
- Sadece Identity RoleClaims (tenant bazlý varyasyon yok)
- Rol patlamasý (HeadCoach vb.)
