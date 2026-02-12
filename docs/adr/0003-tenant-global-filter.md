# ADR-0003: Tenant Isolation via TenantScopedEntity + Global Query Filter

**Durum:** Accepted  
**Tarih:** 2026-02-12

## Context
- Tenant veri sızıntısını engellemek kritik.
- Kod tekrarını ve handler’larda tenant check yazma ihtiyacını azaltmak istiyoruz.

## Decision
- Tenant-owned tüm entity’ler `TenantScopedEntity` olacak (TenantId zorunlu).
- EF Core Global Query Filter: `TenantId == CurrentTenantId`
- Tenant verisi içeren modüller tenant context (CurrentTenantId) olmadan çalışmaz.
- Host-level ekranlarda (örn. audit.read.all) gerekiyorsa `IgnoreQueryFilters()` kullanılabilir.

## Consequences
- TenantId migration’ları gerekli.
- Tenant context enforcement (API middleware/MVC filter) gereklidir; login/switch ve host-level ekranlar exempt tanımlanmalıdır.
