# ADR-0002: Audit (CreatedBy/UpdatedBy + AuditEvent via SaveChanges Interceptor)

**Durum:** Accepted  
**Tarih:** 2026-02-12

## Context
- Kimin neyi değiştirdiğini geçmişe dönük görebilmek gerekiyor.
- Handler’lara audit yazma kodu koymak hata riskini artırıyor ve sürdürülebilir değil.

## Decision
- TenantScopedEntity üzerinde: `CreatedByUserId` / `UpdatedByUserId` kolonları.
- Değişiklik geçmişi için `AuditEvent` tablosu.
- Audit yazımı EF Core SaveChanges interceptor ile otomatik yapılır.
- Okuma yetkileri:
  - `audit.read.all` sadece SuperAdmin
  - `audit.read.tenant` SuperAdmin’in yetkilendirdiği admin

## Consequences
- DB büyümesi için retention/arşivleme politikası gerekebilir.
- Hassas alanlar (password hash, token, secret vb.) audit’e dahil edilmemelidir.
