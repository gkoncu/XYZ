# ADR-0002: Audit (CreatedBy/UpdatedBy + AuditEvent via SaveChanges Interceptor)

**Durum:** Proposed | Accepted | Superseded  
**Tarih:** YYYY-MM-DD

## Context
- Kimin neyi deðiþtirdiðini geçmiþe dönük görmek
- Handler’larda audit kodu istemiyoruz

## Decision
- TenantScopedEntity: CreatedBy/UpdatedBy kolonlarý
- AuditEvent tablosu
- SaveChanges interceptor ile otomatik yazým
- audit.read.all (superadmin), audit.read.tenant (designated)

## Consequences
- DB büyümesi, retention politikasý gerekebilir
- Hassas alanlar ignore edilmelidir
