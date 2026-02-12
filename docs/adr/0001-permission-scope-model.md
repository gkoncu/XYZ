# ADR-0001: Permission + Scope (Tenant Role Template + User Override)

**Durum:** Accepted  
**Tarih:** 2026-02-12

## Context
- SaaS modelinde tenant bazlı farklı ihtiyaçlar var.
- Aynı rolde kullanıcı bazlı farklı yetki/scope ihtiyacı var.
- Handler’larda rol string kontrolünü azaltmak ve yetkilendirmeyi merkezi hale getirmek istiyoruz.

## Decision
- Identity Role = varsayılan “şablon”.
- Tenant bazlı rol şablonları: `TenantRolePermission`
- Tenant bazlı kullanıcı override: `TenantUserPermissionOverride`
- Çözümleme sonucu: `EffectivePermissionSet` (cache + versioning ile).
- Scope seviyeleri: Self / OwnClasses / Branch / Tenant / AllTenants
- “Deny wins” yaklaşımı kullanılmayacak; governance (permissions.manage + protected admin + superadmin) ile korunacak.

## Consequences
- Tenant’a göre rol davranışı ve aynı rolde kullanıcı bazlı farklı yetkiler mümkün olur.
- UI’da rol şablonu + kullanıcı override yönetimi gerekir.
- EffectivePermissionSet cache invalidation için versioning gerekir.

## Alternatives considered
- Sadece Identity RoleClaims (tenant bazlı varyasyon yok, esneklik düşük)
- Rol patlaması (HeadCoach vb. için yeni rol üretme; yönetim maliyeti yüksek)
