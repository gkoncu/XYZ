# SSOT — Tenant Isolation + Permission/Scope Authorization + Audit

**Proje:** XYZ App  
**Sahip:** (Team / Owner)  
**Durum:** Draft | Accepted  
**Son Güncelleme:** YYYY-MM-DD  
**Kapsam:** Domain / Application / API / WebMVC (Authorization, Scoping, Audit, Governance)

> Bu doküman **Single Source of Truth**’tur.
> Yetki, kapsam, tenant izolasyonu ve audit ile ilgili tüm kararlar burada tanýmlanýr.
> Kod deðiþikliði bu standartlarý etkiliyorsa, bu dosya + ilgili ADR güncellenmeden PR merge edilmez.

---

## 0) Hedefler

- Handler’larda rol string check / daðýnýk if kontrollerini kaldýrmak.
- Yetkilendirmeyi merkezi ve test edilebilir hale getirmek.
- Tenant izolasyonunu EF Core Global Query Filter ile garantilemek.
- Rol gerekli kalsýn; ancak yetkiler tenant bazlý deðiþebilsin ve kullanýcý bazlý override desteklensin.
- Row-level (hangi veriler) + action-level (hangi iþlemler) ayrýmýný net tutmak.
- Audit: kim neyi ne zaman deðiþtirdi + deðiþiklik geçmiþi.
- Performans: Effective permission set caching + versioning.
- Explainability: “Bu kullanýcý neden bunu yapabiliyor?” izahý.

---

## 1) Terimler ve Sözlük

- **Tenant:** Kulüp (izolasyon birimi).
- **Role:** Identity rolü (Admin/Coach/Finance/Student/SuperAdmin). “Þablon” görevi görür.
- **Permission (Action-level):** “Ne yapabilir?” (örn. `students.assignClass`)
- **Scope (Row-level):** “Hangi veri üzerinde?” (Self/OwnClasses/Branch/Tenant/AllTenants)
- **EffectivePermissionSet:** Role template + user override birleþimi sonucu.
- **Protected Admin:** SuperAdmin tarafýndan “korunmuþ” olarak provision edilen admin; tenant içindeki diðer adminler tarafýndan edit/sil/yetki deðiþimi yapýlamaz.
- **Global Query Filter:** `TenantId == CurrentTenantId` koþulunu otomatik uygular.

---

## 2) Ýlkeler

1) **Tenant izolasyonu her zaman otomatik**: Kodun hiçbir yerinde “tenant id check” tekrarý yapýlmaz (istisnalar dokümanda belirtilir).
2) **Action-level authorization** role string’leriyle handler’da yapýlmaz; policy/permission ile yapýlýr.
3) **Row-level scoping** DataScope ile yapýlýr; role switch ile deðil, scope ile uygulanýr.
4) **Business/UX filtreleri** (eligible listeler, search/paging) DataScope’a girmez.
5) **Standart isimlendirme**: RoleNames / PermissionNames sabitleri zorunludur.
6) **Audit** otomatik yazýlýr; handler içine “audit log” kodu girmez.

---

## 3) Tenant Isolation Standardý

### 3.1 TenantScopedEntity yaklaþýmý
- Tenant’a ait tüm entity’ler `TenantScopedEntity`’den türemelidir.
- `TenantScopedEntity` alaný:
  - `TenantId` (zorunlu, non-null)

> Not: Tenant entity’si kendi içinde tenantId taþýmaz. Tenant “host-level” olarak deðerlendirilir.

### 3.2 TenantId migration kuralý
- Domain’de tenant’a ait olup TenantId taþýmayan entity kalmayacak.
- Örnek (tam liste projeye göre güncellenecek):
  - Students, Coaches, Classes, ClassSessions, Attendance, Payments, PaymentPlans, Documents, ...
- Ýstisnalar:
  - Auth/RefreshToken gibi tenant seçimi öncesi çalýþan yapýlar (netleþtirilecek).

### 3.3 EF Core Global Query Filter
- Tüm `TenantScopedEntity` için:
  - `e => e.TenantId == _current.TenantId` (CurrentTenantId)
- SuperAdmin tenant switch yaptýktan sonra CurrentTenantId set edilir ve global filter otomatik çalýþýr.

**Kural:** `_current.TenantId` uygulama modüllerinde her request’te dolu olmalýdýr.  
Tenant context yoksa request reddedilir (login/switch endpointleri hariç).

---

## 4) Permission + Scope Yetkilendirme Modeli

### 4.1 Scope sözlüðü
- `Self`
- `OwnClasses`
- `Branch`
- `Tenant`
- `AllTenants` (host-only)

Opsiyonel:
- `ScopeRefId` (örn. BranchId gibi “hangi branch?”)

### 4.2 Permission isimlendirme standardý
- Format: `module.action` (örn. `students.assignClass`)
- Her permission için sabit alan: `PermissionNames.*`
- Role isimleri: `RoleNames.*`

### 4.3 Veri modeli (tablo tasarýmý)

#### A) TenantRolePermission (tenant bazlý rol þablonu)
- TenantId
- RoleName
- PermissionKey
- ScopeLevel
- ScopeRefId? (opsiyonel)
- CreatedAt/CreatedBy (opsiyonel)
- UpdatedAt/UpdatedBy (opsiyonel)

#### B) TenantUserPermissionOverride (tenant bazlý kullanýcý override)
- TenantId
- UserId
- PermissionKey
- ScopeLevel
- ScopeRefId? (opsiyonel)
- CreatedAt/CreatedBy
- UpdatedAt/UpdatedBy

> Not: “Deny” mantýðý kullanýlmayacak (SSOT kararý). Yetki yönetimi sadece ilgili admin + superadmin governance ile korunur.

### 4.4 EffectivePermissionSet çözümleme
- Input:
  - UserId, TenantId
  - Identity Role(leri)
- Çözüm:
  1) TenantRolePermission: role(ler) üzerinden topla
  2) TenantUserPermissionOverride: user override ile güncelle
  3) Sonuç: `EffectivePermissionSet`

### 4.5 Cache + versioning
- `EffectivePermissionSet` cache’lenir (Key: TenantId + UserId)
- Yetki deðiþiminde tenant bazlý veya user bazlý version artýrýlýr (örn. `PermissionsVersion`)
- Version mismatch olduðunda cache invalid edilir.

### 4.6 Explainability
- Endpoint/Service:
  - “Kullanýcý X þu permission’a neden sahip?” açýklamasý:
    - Role template kaynaklarý
    - User override kaynaklarý
    - Scope seviyesi nereden geldi
- UI’da debug amaçlý görünür.

---

## 5) Action-level Authorization (Policy / MediatR Pipeline)

### 5.1 Hedef
Handler içinde:
- `if(role != ...) throw ...` yapýlmaz.

Bunun yerine:
- Policy/permission ile merkezi kontrol.

### 5.2 Uygulama
- MediatR pipeline:
  - Request’te ihtiyaç duyulan permission metadata’sý bulunur.
  - `_authorization.Ensure(permissionKey)` çalýþýr.
- Alternatif: API attribute policy (kademeli).

### 5.3 Policy standardý (örnek)
- Policy adý permission key ile ayný tutulabilir:
  - `students.assignClass`

### 5.4 Kritik governance yetkileri
- `permissions.manage`:
  - Sadece SuperAdmin verebilir/geri alabilir.
- “Protected Admin” deðiþiklikleri:
  - Sadece SuperAdmin yapabilir.

---

## 6) Row-level Scoping (DataScope)

### 6.1 Hedef
- Query her zaman “scope uygulanmýþ” baþlar.
- Role switch yapýlmaz.

### 6.2 Örnek yaklaþým
- `DataScope.Students(permissionKey?)` / `DataScope.Students()`:
  - EffectivePermissionSet’ten ilgili permission’ýn scope’u bulunur
  - Scope’a göre expression uygulanýr

### 6.3 Scope uygulama örnekleri (temel)
- Self:
  - Student: `s.Id == _current.StudentId`
  - Coach: `c.Id == _current.CoachId`
- OwnClasses:
  - Students: `s.Class.Coaches.Any(co => co.Id == _current.CoachId)`
  - Classes: `c.Coaches.Any(co => co.Id == _current.CoachId)`
- Branch:
  - `Entity.BranchId == assignedBranchId` (ScopeRefId veya user profile)
- Tenant:
  - (Zaten global filter ile tenant izolasyonu var) + ek kriter yok
- AllTenants:
  - Sadece host-level endpointlerde kullanýlýr

### 6.4 Business filtreleri bu katmana girmez
Örn:
- “ClassId null olan öðrenciler” eligible filtresi
- Search/paging/sort
Bunlar ilgili query handler’da uygulanýr.

---

## 7) Governance (SuperAdmin, Protected Admin, Permission yönetimi)

### 7.1 Protected Admin
- SuperAdmin bir admini “Protected” olarak provision edebilir.
- Protected admin:
  - Tenant içindeki diðer adminler tarafýndan editlenemez/silinemez/yetkisi deðiþtirilemez.
- Bu kural:
  - User–Tenant membership üzerinde tutulur (örn. `IsProtected`).

### 7.2 Permission yönetimi
- `permissions.manage` yetkisini yalnýz SuperAdmin verebilir.
- Bu yetkiye sahip admin, tenant içindeki diðer permission’larý yönetebilir.

---

## 8) Audit Standardý

### 8.1 Entity alanlarý
`TenantScopedEntity` üzerinde:
- `CreatedAt`, `UpdatedAt`
- `CreatedByUserId`, `UpdatedByUserId`

### 8.2 AuditEvent tablosu
- TenantId
- EntityName
- EntityKey (string)
- Action (Create/Update/Delete/SoftDelete)
- ActorUserId
- OccurredAt
- ChangesJson (opsiyonel: old/new veya sadece deðiþen alan adlarý)

### 8.3 Yazým mekanizmasý
- EF Core SaveChanges interceptor ile otomatik.
- Handler’larda audit yazýlmaz.

### 8.4 Okuma yetkileri
- `audit.read.all`: sadece SuperAdmin (AllTenants)
- `audit.read.tenant`: “tenant top admin” veya superadmin’in yetkilendirdiði admin

### 8.5 Hassas alanlar
- Password hash, secret, token gibi alanlar audit’e yazýlmaz (ignore list).

---

## 9) UI / Yönetim Ekranlarý (Minimum)
- Role template yönetimi (TenantRolePermission)
- User override yönetimi (TenantUserPermissionOverride)
- Protected admin bayraðý (SuperAdmin ekraný)
- Explainability ekraný (“neden yetkili?”)

---

## 10) Test Standardý
- Permission çözümleme unit test:
  - role template + override birleþimi
- DataScope test:
  - her scope seviyesi için örnek query sonuçlarý
- Governance test:
  - Protected admin edit/delete block
  - permissions.manage yalnýz superadmin’den gelebilir
- Global query filter test:
  - TenantId sýzýntýsý yok

---

## 11) Göç/Migration Checklist (sýra ile)
1) TenantId eksik entity’lere ekle
2) TenantScopedEntity ayrýmý
3) Global query filter
4) CreatedBy/UpdatedBy kolonlarý
5) AuditEvent tablosu + interceptor
6) Permission tablolarý
7) EffectivePermissionSet resolver + cache/version
8) Action-level auth (pipeline/policy)
9) Modül modül refactor

---

## 12) Açýk Konular
- RefreshTokenEntity tenant’a baðlý mý? (login/switch akýþýna göre)
- Branch scope için ScopeRefId nereden gelir? (user profile vs grant)
- “Tenant top admin” tanýmý: IsPrimaryAdmin mý, ayrý permission mý?
- Audit ChangesJson kapsamý: full old/new mý, minimal mi?
