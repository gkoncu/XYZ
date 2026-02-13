# SSOT — Tenant Isolation + Permission/Scope Authorization + Audit

**Proje:** XYZ App  
**Sahip:** (Team / Owner)  
**Durum:** Accepted  
**Son Güncelleme:** 2026-02-12  
**Kapsam:** Domain / Application / API / WebMVC (Authorization, Scoping, Audit, Governance)

> Bu doküman **Single Source of Truth**’tur (SSOT).  
> Yetki, kapsam, tenant izolasyonu ve audit ile ilgili tüm kararlar burada tanımlanır.  
> Bu standartları etkileyen her değişiklikte bu dosya + ilgili ADR güncellenir.

---

## 0) Hedefler

- Handler’larda rol string check / dağınık `if` kontrollerini kaldırmak.
- Yetkilendirmeyi merkezi ve test edilebilir hale getirmek.
- Tenant izolasyonunu EF Core Global Query Filter ile garanti etmek.
- Rol (Identity) gerekli kalsın; ancak **yetkiler tenant bazlı değişebilsin** ve **kullanıcı bazlı override** desteklensin.
- Row-level (hangi veriler) + action-level (hangi işlemler) ayrımını net tutmak.
- Audit: kim, neyi, ne zaman değiştirdi + değişiklik geçmişi.
- Performans: Effective permission set caching + versioning.
- Explainability: “Bu kullanıcı neden bunu yapabiliyor?” izahı.

---

## 1) Terimler ve Sözlük

- **Tenant:** Kulüp (izolasyon birimi).
- **Role:** Identity rolü (Admin/Coach/Finance/Student/SuperAdmin). Varsayılan yetki şablonu olarak kullanılır.
- **Permission (Action-level):** “Ne yapabilir?” (örn. `students.assignClass`)
- **Scope (Row-level):** “Hangi veri üzerinde?” (Self/OwnClasses/Branch/Tenant/AllTenants)
- **EffectivePermissionSet:** Tenant role template + user override çözümlemesi sonucu ortaya çıkan efektif yetki kümesi.
- **Protected Admin:** SuperAdmin tarafından “korunmuş” olarak işaretlenen admin; tenant içindeki diğer adminler tarafından edit/sil/yetki değişimi yapılamaz.
- **Global Query Filter:** `TenantId == CurrentTenantId` filtresinin EF Core tarafından otomatik uygulanması.

---

## 2) İlkeler

1) **Tenant izolasyonu otomatik olmalı.** Kodun hiçbir yerinde “tenant id check” tekrarı yapılmaz (istisnalar SSOT’ta).
2) **Action-level authorization** handler’larda role string ile yapılmaz; policy/permission ile yapılır.
3) **Row-level scoping** DataScope ile yapılır; role switch ile değil, scope ile uygulanır.
4) **Business/UX filtreleri** (eligible listeler, search/paging) DataScope’a girmez.
5) **Standart isimlendirme**: `RoleNames` / `PermissionNames` sabitleri zorunludur.
6) **Audit otomatik yazılır.** Handler içine “audit log” kodu girmez.

---

## 3) Tenant Isolation Standardı

### 3.1 TenantScopedEntity yaklaşımı

- Tenant’a ait tüm entity’ler `TenantScopedEntity`’den türemelidir.
- `TenantScopedEntity` alanı:
  - `TenantId` (**zorunlu**, non-null)

> Not: `Tenant` entity’si host-level’dir; TenantScopedEntity değildir.

### 3.2 Tenant-owned Entity Listesi (TenantScopedEntity)

Aşağıdaki entity’ler tenant verisi taşır ve `TenantScopedEntity` olmalıdır:

- Admin
- Announcement
- Attendance
- Branch
- Class
- ClassEnrollment
- ClassSession
- Coach
- Document
- DocumentDefinition
- Payment
- PaymentPlan
- ProgressMetricDefinition
- ProgressRecord
- ProgressRecordValue
- Student

Identity tarafı:
- ApplicationUser tenant-scoped kabul edilir; Identity entity olduğu için TenantScopedEntity’den türemez. TenantId/claim + tenant context enforcement ile korunur.

Host-level (TenantScopedEntity değildir):
- Tenant

Auth-level / özel:
- RefreshTokenEntity: login/refresh/switch akışına göre TenantId zorunlu veya opsiyonel olabilir.

> Not (mevcut durum): projede şu entity’lerde TenantId henüz yok (dolaylı tenant var). SSOT gereği bu entity’lere TenantId eklenecek:  
> Attendance, ClassEnrollment, ClassSession, Document, ProgressRecord, ProgressRecordValue

### 3.3 TenantId migration kuralı

- Tenant-owned olup TenantId taşımayan entity kalmayacak.
- Migration’dan sonra tenant izolasyonu **global filter** ile otomatik garanti edilir.

### 3.4 EF Core Global Query Filter

- Tüm `TenantScopedEntity` için:
  - `e => e.TenantId == CurrentTenantId`

### 3.5 Tenant Context (CurrentTenantId) zorunluluğu

Bu projede tenant verisi içeren tüm modüller çalışmak için `CurrentTenantId` ister.

- `CurrentTenantId` yoksa:
  - Tenant-scoped modül endpoint’leri **reddedilir** (API) veya kullanıcı **tenant seçme ekranına yönlendirilir** (MVC).
- `CurrentTenantId` varsa:
  - Global query filter otomatik uygulanır.

**Kural:** Uygulama modüllerinde handler/service seviyesinde `TenantId.HasValue` kontrolü yapılmaz. Bu kontrol tek yerde (Tenant Context Enforcement) yapılır.

### 3.6 Host-level işlemler (tenant-scoped olmayan ekranlar)

Bazı işlemler tenant bağlamı gerektirmez; “host-level” kabul edilir:

- Tenant yönetimi: `tenants.read`, `tenants.manage`, `tenants.switch`
- SuperAdmin governance: `users.protectAdmin`, `permissions.manage`
- Global audit okuma: `audit.read.all`

Host-level sorgularda tenant-scoped entity’ler üzerinde tüm tenant’ları görmek gerekirse:
- `IgnoreQueryFilters()` kullanılabilir, ardından explicit filtre/kurallar uygulanır.

---

## 4) Permission + Scope Yetkilendirme Modeli

### 4.1 Scope sözlüğü

- `Self`
- `OwnClasses`
- `Branch` (opsiyonel; ScopeRefId gerekebilir)
- `Tenant`
- `AllTenants` (host-only)

### 4.2 Permission isimlendirme standardı

- Format: `module.action` (örn. `students.assignClass`)
- Tüm permission key’leri `PermissionNames` sabitlerinden alınır (policy adı = permission key).

### 4.3 Veri modeli (tablo tasarımı)

#### A) TenantRolePermission (tenant bazlı rol şablonu)
- TenantId
- RoleName
- PermissionKey
- ScopeLevel
- ScopeRefId? (opsiyonel)
- Audit alanları (CreatedAt/By, UpdatedAt/By) (opsiyonel)

#### B) TenantUserPermissionOverride (tenant bazlı kullanıcı override)
- TenantId
- UserId
- PermissionKey
- ScopeLevel
- ScopeRefId? (opsiyonel)
- Audit alanları (CreatedAt/By, UpdatedAt/By)

**Kural:** “Deny” yaklaşımı kullanılmayacak. Yetki yönetimi governance ile korunur (permissions.manage + protected admin + superadmin).

### 4.4 EffectivePermissionSet çözümleme

- Input: UserId, TenantId, Identity Role(leri)
- Çözüm:
  1) TenantRolePermission üzerinden role template’leri topla
  2) TenantUserPermissionOverride ile override et
  3) Sonuç: EffectivePermissionSet

**SuperAdmin:** tüm permission’lara sahiptir (`PermissionNames.All`).

### 4.5 Cache + versioning

- EffectivePermissionSet cache’lenir (Key: TenantId + UserId)
- Yetki değişiminde version artırılır (örn. tenant/user `PermissionsVersion`)
- Version mismatch → cache invalidate

### 4.6 Explainability

- Servis/endpoint: “Bu kullanıcı bu permission’a neden sahip?”
  - Role template kaynakları
  - User override kaynakları
  - Scope seviyesi kaynağı

### 4.7 Permission ayrımı: `students.assignClass` vs `classes.enrollStudents`

- `students.assignClass`:
  - Öğrencinin sınıf atamasını/değişikliğini yapar (domain state change).

- `classes.enrollStudents`:
  - Sınıf üyeliğinin operasyonel yan etkilerini yönetir:
    - Öğrenciyi gelecekteki yoklama listelerine dahil eder
    - Sınıftan çıkarılan öğrenciyi gelecekteki yoklamalardan çıkarır
    - Geçmiş yoklamalar korunur (history bozulmaz)

Bu iki permission ayrı kalır. Bazı kulüplerde “atamayı yapan” ile “geleceğe yansıtma operasyonunu yapan” ayrıştırılabilir.

### 4.8 Default Tenant Role Templates (Seed)

Scope varsayılanları:
- Admin: Tenant
- Finance: Tenant
- Coach: OwnClasses
- Student: Self
- SuperAdmin: tüm permission’lar (host dahil), tenant verisi için tenant context gerekir.

**Admin (Default)**
- Tenant içi tüm iş modülleri (tenant-scoped permission’ların tamamı).
- Hariç / superadmin-only governance:
  - `permissions.manage` (grant/revoke yalnız SuperAdmin)
  - `users.protectAdmin` (yalnız SuperAdmin)
  - `audit.read.all` (yalnız SuperAdmin)
  - `tenants.*` (host-level, yalnız SuperAdmin)
- `audit.read.tenant` default admin’e verilmek zorunda değil; “Top Admin” olarak işaretlenen kullanıcıya ayrıca verilebilir.

**Coach (Default) — scope=OwnClasses**
- announcements: read.public + read (Tenant)
- classes: read + update (**karar: update default açık**) (OwnClasses)
- attendance: read/take/edit (OwnClasses)
- students: read + attendance.read (OwnClasses)
- documents: read/upload (OwnClasses)
- profile.*.self (Self)

Kulübe göre opsiyonel (default kapalı, user override ile verilebilir):
- payments.read (OwnClasses)
- students.assignClass / classes.enrollStudents
- students.update

**Finance (Default) — scope=Tenant**
- announcements: read.public + read (Tenant)
- payments: read/createPlan/updatePlan/recordPayment/reports.read/export (Tenant)
- payments.adjust (Tenant) default kapalı önerilir (iade/iptal kritik)
- students: read + payments.read (Tenant)
- reports: read + export (Tenant)
- profile.*.self (Self)

**Student (Default) — scope=Self**
- announcements: read.public + read (Tenant)
- students: read + attendance.read + payments.read (Self)
- documents: read/upload + students.documents.* (Self)
- profile.*.self (Self)

---

## 5) Action-level Authorization (Policy / MediatR Pipeline)

### 5.1 Hedef
Handler içinde role string ile kontrol yapılmaz:
- `if(role != ...) throw ...` yok.

### 5.2 Uygulama
- MediatR pipeline veya policy-based authorization:
  - Request’in ihtiyaç duyduğu permission belirlenir
  - `_authorization.Ensure(permissionKey)` çalışır

### 5.3 Policy standardı
- Policy adı permission key ile aynı tutulabilir:
  - `students.assignClass`

### 5.4 Kritik governance permission’ları
- `permissions.manage`: yalnız SuperAdmin grant/revoke eder.
- `users.protectAdmin`: yalnız SuperAdmin.

### 5.5 Tenant Context Enforcement Standardı

Tenant-scoped endpoint’ler çalışmadan önce `CurrentTenantId` varlığı merkezi olarak doğrulanır.

- API: middleware/filter ile request başında kontrol
- MVC: action filter ile kontrol (gerekirse tenant seçme sayfasına yönlendirme)

Login / tenant listesi / tenant switch gibi endpoint’ler bu kontrolden muaftır.

#### Tenant-scoped — API Controllers
Tenant context zorunlu (CurrentTenantId yoksa reject):
- AdminsController
- AnnouncementsController
- AttendancesController
- BranchesController
- ClassSessionsController
- ClassesController
- CoachesController
- DashboardController
- DocumentDefinitionsController
- DocumentsController
- PaymentPlansController
- PaymentsController
- ProfileController
- ProgressMetricDefinitionsController
- ProgressRecordsController
- StudentsController
- TenantsController: `current-theme` endpoint’leri tenant-scoped kabul edilir.

#### Exempt / Host-level — API Controllers
Tenant context zorunlu değildir:
- AuthController (login/refresh/logout vb.)
- TenantsController (tenant CRUD) host-level kabul edilir (SuperAdmin).

#### Tenant-scoped — WebMVC Controllers
Tenant context zorunlu (tenant yoksa yönlendirme/403):
- AdminDashboardController
- AdminsController
- AnnouncementsController
- AttendanceController
- BranchesController
- CalendarController
- ClassSessionsController
- ClassesController
- CoachDashboardController
- CoachesController
- DocumentDefinitionsController
- DocumentsController
- PaymentPlansController
- PaymentsController
- ProfileController
- ProgressMetricDefinitionsController
- ProgressRecordsController
- SettingsController
- StudentDashboardController
- StudentsController
- SuperAdminDashboardController

#### Exempt / Host-level — WebMVC Controllers
Tenant context zorunlu değildir:
- AccountController (login/logout/forgot/set password)
- HomeController (public/landing varsa)
- TenantsController (SuperAdmin tenant yönetimi ekranı)

---

## 6) Row-level Scoping (DataScope)

### 6.1 Hedef
- Query her zaman “scope uygulanmış” başlar.
- Role switch yapılmaz; scope uygulanır.

### 6.2 Yaklaşım
- DataScope, EffectivePermissionSet’ten ilgili permission’ın scope’unu bulur ve expression uygular.
- Örnek:
  - Students query → scope `OwnClasses` ise yalnız koçun sınıflarındaki öğrenciler

### 6.3 Business filtreleri bu katmana girmez
Örn:
- “ClassId null olan öğrenciler” (eligible list)
- Search/paging/sort
Bunlar ilgili query/handler’da uygulanır.

---

## 7) Governance (SuperAdmin, Protected Admin, Permission yönetimi)

### 7.1 Protected Admin (SuperAdmin provisioned)
- SuperAdmin, tenant içindeki bir admini “Protected” olarak işaretleyebilir.
- Protected admin:
  - Tenant içindeki diğer adminler tarafından silinemez
  - Yetkileri/rolü değiştirilemez
  - Kritik profil alanları değiştirilemez
- Bu işlemler yalnız SuperAdmin tarafından yapılabilir.

### 7.2 permissions.manage
- `permissions.manage` yetkisi tenant adminlerine yalnız SuperAdmin tarafından verilebilir/geri alınabilir.
- Bu yetkiye sahip admin:
  - TenantRolePermission ve TenantUserPermissionOverride yönetebilir.

### 7.3 SuperAdmin yetkisi ve tenant davranışı
- SuperAdmin tüm permission’lara sahiptir (tam kontrol).
- Tenant verisi içeren modüller tenant context gerektirir:
  - SuperAdmin bir tenant seçmek zorundadır (switch) — global query filter nedeniyle tenant seçmeden tenant verisi görülemez.
- Host-level ekranlar tenant context gerektirmez:
  - `tenants.*`, `permissions.manage`, `users.protectAdmin`, `audit.read.all` vb.

---

## 8) Audit Standardı

### 8.1 Entity alanları
TenantScopedEntity üzerinde:
- `CreatedAt`, `UpdatedAt`
- `CreatedByUserId`, `UpdatedByUserId`

### 8.2 AuditEvent tablosu
- TenantId
- EntityName
- EntityKey (string)
- Action (Create/Update/Delete/SoftDelete)
- ActorUserId
- OccurredAt
- ChangesJson

**Karar:** ChangesJson default “minimal” (değişen alan adları + new değer). Old değer yalnız gerekli görülen kritik alanlarda opsiyonel açılır.

### 8.3 Yazım mekanizması
- EF Core SaveChanges interceptor ile otomatik.
- Handler’larda audit yazılmaz.

### 8.4 Okuma yetkileri
- `audit.read.all`: sadece SuperAdmin (host)
- `audit.read.tenant`: SuperAdmin’in yetkilendirdiği “Top Admin” vb.

### 8.5 Hassas alanlar
- Password hash, secret, token vb. alanlar audit’e yazılmaz (ignore list).

---

## 9) UI / Yönetim Ekranları (Minimum)

- Role template yönetimi (TenantRolePermission)
- User override yönetimi (TenantUserPermissionOverride)
- Protected admin bayrağı (SuperAdmin ekranı)
- Explainability ekranı (“neden yetkili?”)
- SuperAdmin tenant switch + tenant yönetimi

---

## 10) Test Standardı

- EffectivePermissionSet resolver unit test
- DataScope unit/integration test (Self/OwnClasses/Tenant senaryoları)
- Governance test:
  - Protected admin edit/delete block
  - permissions.manage grant/revoke yalnız SuperAdmin
- Global query filter test: tenant sızıntısı yok
- Tenant context enforcement test: tenant yoksa reject/redirect

---

## 11) Göç/Migration Checklist (sıra ile)

1) TenantScopedEntity + TenantId eksik entity’lere ekleme
2) Global Query Filter
3) Tenant context enforcement (API middleware / MVC filter) + exempt list
4) Permission tabloları (TenantRolePermission, TenantUserPermissionOverride)
5) EffectivePermissionSet resolver + cache/versioning + explainability
6) Action-level authorization (policy/pipeline)
7) DataScope dönüşümü (scope bazlı)
8) Audit: CreatedBy/UpdatedBy + AuditEvent + interceptor
9) Modül modül refactor (authorization + datascope + UI görünürlük)

---

## 12) Açık Konular

- RefreshTokenEntity TenantId zorunlu mu opsiyonel mi? (auth akışına göre)
- Branch scope (ScopeRefId) kaynağı: user profile mı, grant mi?
- “Top Admin” tanımı: ayrı flag mi, ayrı permission mı?
- Audit retention (saklama süresi) / arşivleme politikası
