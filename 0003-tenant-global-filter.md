# ADR-0003: Tenant Isolation via TenantScopedEntity + Global Query Filter

**Durum:** Proposed | Accepted | Superseded  
**Tarih:** YYYY-MM-DD

## Context
- Tenant veri sýzýntýsýný engelleme
- Kod tekrarýný azaltma

## Decision
- TenantScopedEntity tüm tenant-owned entity’lerde
- EF Core Global Query Filter: TenantId == CurrentTenantId
- SuperAdmin tenant switch ile CurrentTenantId set eder

## Consequences
- TenantId migration’larý gerekli
- Tenant context olmayan endpoint’ler istisna olarak ele alýnmalý (login/switch)
