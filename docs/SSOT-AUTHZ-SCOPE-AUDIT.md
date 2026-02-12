# SSOT — Tenant Isolation + Permission/Scope Authorization + Audit

**Proje:** XYZ App  
**Sahip:** (Team / Owner)  
**Durum:** Draft | Accepted  
**Son Güncelleme:** YYYY-MM-DD  
**Kapsam:** Domain / Application / API / WebMVC (Authorization, Scoping, Audit, Governance)

> Bu doküman **Single Source of Truth**’tur.
> Yetki, kapsam, tenant izolasyonu ve audit ile ilgili tüm kararlar burada tanımlanır.
> Kod değişikliği bu standartları etkiliyorsa, bu dosya + ilgili ADR güncellenmeden PR merge edilmez.

---

## 0) Hedefler

- Handler’larda rol string check / dağınık if kontrollerini kaldırmak.
- Yetkilendirmeyi merkezi ve test edilebilir hale getirmek.
- Tenant izolasyonunu EF Core Global Query Filter ile garantilemek.
- Rol gerekli kalsın; ancak yetkiler tenant bazlı değişebilsin ve kullanıcı bazlı override desteklensin.
- Row-level (hangi veriler) + action-level (hangi işlemler) ayrımını net tutmak.
- Audit: kim neyi ne zaman değiştirdi + değişiklik geçmişi.
- Performans: Effective permission set caching + versioning.
- Explainability: “Bu kullanıcı neden bunu yapabiliyor?” izahı.

---

## 1) Terimler ve Sözlük

- **Tenant:** Kulüp (izolasyon birimi).
- **Role:** Identity rolü (Admin/Coach/Finance/Student/SuperAdmin). “Şablon” görevi görür.
- **Permission (Action-level):** “Ne yapabilir?” (örn. `students.assignClass`)
- **Scope (Row-level):** “Hangi veri üzerinde?” (Self/OwnClasses/Branch/Tenant/AllTenants)
- **EffectivePermissionSet:** Role template + user override birleşimi sonucu.
- **Protected Admin:** SuperAdmin tarafından “korunmuş” olarak provision edilen admin; tenant içindeki diğer adminler tarafından edit/sil/yetki değişimi yapılamaz.
- **Global Query Filter:** `TenantId == CurrentTenantId` koşulunu otomatik uygular.

---

## 2) İlkeler

1) **Tenant izolasyonu her zaman otomatik**: Kodun hiçbir yerinde “tenant id check” tekrarı yapılmaz (istisnalar dokümanda belirtilir).
2) **Action-level authorization** role string’leriyle handler’da yapılmaz; policy/permission ile yapılır.
3) **Row-level scoping** DataScope ile yapılır; role switch ile değil, scope ile uygulanır.
4) **Business/UX filtreleri** (eligible listeler, search/paging) DataScope’a girmez.
5) **Standart isimlendirme**: RoleNames / PermissionNames sabitleri zorunludur.
6) **Audit** otomatik yazılır; handler içine “audit log” kodu girmez.

---

## 3) Tenant Isolation Standardı

### 3.1 TenantScopedEntity yaklaşımı
- Tenant’a ait tüm entity’ler `TenantScopedEntity`’den türemelidir.
- `TenantScopedEntity` alanı:
  - `TenantId` (zorunlu, non-null)

> Not: Tenant entity’si kendi içinde tenantId taşımaz. Tenant “host-level” olarak değerlendirilir.

### 3.2 TenantId migration kuralı
- Domain’de tenant’a ait olup TenantId taşımayan entity kalmayacak.
- Örnek (tam liste projeye göre güncellenecek):
  - Students, Coaches, Classes, ClassSessions, Attendance, Payments, PaymentPlans, Documents, ...
- İstisnalar:
  - Auth/RefreshToken gibi tenant seçimi öncesi çalışan yapılar (netleştirilecek).

### 3.3 EF Core Global Query Filter
- Tüm `TenantScopedEntity` için:
  - `e => e.TenantId == _current.TenantId` (CurrentTenantId)
- SuperAdmin tenant switch yaptıktan sonra CurrentTenantId set edilir ve global filter otomatik çalışır.

**Kural:** `_current.TenantId` uygulama modüllerinde her request’te dolu olmalıdır.  
Tenant context yoksa request reddedilir (login/switch endpointleri hariç).

---

## 4) Permission + Scope Yetkilendirme Modeli

### 4.1 Scope sözlüğü
- `Self`
- `OwnClasses`
- `Branch`
- `Tenant`
- `AllTenants` (host-only)

Opsiyonel:
- `ScopeRefId` (örn. BranchId gibi “hangi branch?”)

### 4.2 Permission isimlendirme standardı
- Format: `module.action` (örn. `students.assignClass`)
- Her permission için sabit alan: `PermissionNames.*`
- Role isimleri: `RoleNames.*`

### 4.3 Veri modeli (tablo tasarımı)

#### A) TenantRolePermission (tenant bazlı rol şablonu)
- TenantId
- RoleName
- PermissionKey
- ScopeLevel
- ScopeRefId? (opsiyonel)
- CreatedAt/CreatedBy (opsiyonel)
- UpdatedAt/UpdatedBy (opsiyonel)

#### B) TenantUserPermissionOverride (tenant bazlı kullanıcı override)
- TenantId
- UserId
- PermissionKey
- ScopeLevel
- ScopeRefId? (opsiyonel)
- CreatedAt/CreatedBy
- UpdatedAt/UpdatedBy

> Not: “Deny” mantığı kullanılmayacak (SSOT kararı). Yetki yönetimi sadece ilgili admin + superadmin governance ile korunur.

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
- Yetki değişiminde tenant bazlı veya user bazlı version artırılır (örn. `PermissionsVersion`)
- Version mismatch olduğunda cache invalid edilir.

### 4.6 Explainability
- Endpoint/Service:
  - “Kullanıcı X şu permission’a neden sahip?” açıklaması:
    - Role template kaynakları
    - User override kaynakları
    - Scope seviyesi nereden geldi
- UI’da debug amaçlı görünür.

---

## 5) Action-level Authorization (Policy / MediatR Pipeline)

### 5.1 Hedef
Handler içinde:
- `if(role != ...) throw ...` yapılmaz.

Bunun yerine:
- Policy/permission ile merkezi kontrol.

### 5.2 Uygulama
- MediatR pipeline:
  - Request’te ihtiyaç duyulan permission metadata’sı bulunur.
  - `_authorization.Ensure(permissionKey)` çalışır.
- Alternatif: API attribute policy (kademeli).

### 5.3 Policy standardı (örnek)
- Policy adı permission key ile aynı tutulabilir:
  - `students.assignClass`

### 5.4 Kritik governance yetkileri
- `permissions.manage`:
  - Sadece SuperAdmin verebilir/geri alabilir.
- “Protected Admin” değişiklikleri:
  - Sadece SuperAdmin yapabilir.

---

## 6) Row-level Scoping (DataScope)

### 6.1 Hedef
- Query her zaman “scope uygulanmış” başlar.
- Role switch yapılmaz.

### 6.2 Örnek yaklaşım
- `DataScope.Students(permissionKey?)` / `DataScope.Students()`:
  - EffectivePermissionSet’ten ilgili permission’ın scope’u bulunur
  - Scope’a göre expression uygulanır

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
  - Sadece host-level endpointlerde kullanılır

### 6.4 Business filtreleri bu katmana girmez
Örn:
- “ClassId null olan öğrenciler” eligible filtresi
- Search/paging/sort
Bunlar ilgili query handler’da uygulanır.

---

## 7) Governance (SuperAdmin, Protected Admin, Permission yönetimi)

### 7.1 Protected Admin
- SuperAdmin bir admini “Protected” olarak provision edebilir.
- Protected admin:
  - Tenant içindeki diğer adminler tarafından editlenemez/silinemez/yetkisi değiştirilemez.
- Bu kural:
  - User–Tenant membership üzerinde tutulur (örn. `IsProtected`).

### 7.2 Permission yönetimi
- `permissions.manage` yetkisini yalnız SuperAdmin verebilir.
- Bu yetkiye sahip admin, tenant içindeki diğer permission’ları yönetebilir.

---

## 8) Audit Standardı

### 8.1 Entity alanları
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
- ChangesJson (opsiyonel: old/new veya sadece değişen alan adları)

### 8.3 Yazım mekanizması
- EF Core SaveChanges interceptor ile otomatik.
- Handler’larda audit yazılmaz.

### 8.4 Okuma yetkileri
- `audit.read.all`: sadece SuperAdmin (AllTenants)
- `audit.read.tenant`: “tenant top admin” veya superadmin’in yetkilendirdiği admin

### 8.5 Hassas alanlar
- Password hash, secret, token gibi alanlar audit’e yazılmaz (ignore list).

---

## 9) UI / Yönetim Ekranları (Minimum)
- Role template yönetimi (TenantRolePermission)
- User override yönetimi (TenantUserPermissionOverride)
- Protected admin bayrağı (SuperAdmin ekranı)
- Explainability ekranı (“neden yetkili?”)

---

## 10) Test Standardı
- Permission çözümleme unit test:
  - role template + override birleşimi
- DataScope test:
  - her scope seviyesi için örnek query sonuçları
- Governance test:
  - Protected admin edit/delete block
  - permissions.manage yalnız superadmin’den gelebilir
- Global query filter test:
  - TenantId sızıntısı yok

---

## 11) Göç/Migration Checklist (sıra ile)
1) TenantId eksik entity’lere ekle
2) TenantScopedEntity ayrımı
3) Global query filter
4) CreatedBy/UpdatedBy kolonları
5) AuditEvent tablosu + interceptor
6) Permission tabloları
7) EffectivePermissionSet resolver + cache/version
8) Action-level auth (pipeline/policy)
9) Modül modül refactor

---

## 12) Açık Konular
- RefreshTokenEntity tenant’a bağlı mı? (login/switch akışına göre)
- Branch scope için ScopeRefId nereden gelir? (user profile vs grant)
- “Tenant top admin” tanımı: IsPrimaryAdmin mı, ayrı permission mı?
- Audit ChangesJson kapsamı: full old/new mı, minimal mi?

---

## 13) Sonuç
Announcements: announcements.read.public, announcements.read, announcements.create, announcements.update, announcements.publish, announcements.delete

Attendance: attendance.read, attendance.take, attendance.edit, attendance.reports.read, attendance.export

Audit: audit.read.tenant, audit.read.all

Branches: branches.read, branches.create, branches.update, branches.archive, branches.delete

Classes: classes.read, classes.create, classes.update, classes.archive, classes.delete, classes.assignCoach, classes.enrollStudents, classes.unenrollStudents

Coaches: coaches.read, coaches.create, coaches.update, coaches.archive, coaches.delete, coaches.assignClass

Documents: documents.read, documents.upload, documents.delete, documents.definitions.manage

Payments: payments.read, payments.createPlan, payments.updatePlan, payments.recordPayment, payments.adjust, payments.reports.read, payments.export

Profiles: profile.read.self, profile.update.self, profile.password.change.self

Reports: reports.read, reports.export

Settings: tenant.settings.manage, integrations.manage

Students: students.read, students.create, students.update, students.archive, students.delete, students.assignClass, students.changeClass, students.attendance.read, students.payments.read, students.documents.read, students.documents.manage

Tenants (host): tenants.read, tenants.manage, tenants.switch

Users: users.read, users.create, users.update, users.disable, users.delete, users.protectAdmin

Permissions: permissions.manage, permissions.explain

Default scope konvansiyonu

Admin: Tenant

Finance: Tenant

Coach: OwnClasses

Student: Self

SuperAdmin (host): AllTenants (sadece host yetkileri)

1) Admin (Tenant rol şablonu)

Scope: Tenant (tam yetki – kritik governance hariç)

Announcements

announcements.read.public (Tenant)

announcements.read (Tenant)

announcements.create (Tenant)

announcements.update (Tenant)

announcements.publish (Tenant)

announcements.delete (Tenant)

Attendance

attendance.read (Tenant)

attendance.take (Tenant)

attendance.edit (Tenant)

attendance.reports.read (Tenant)

attendance.export (Tenant)

Branches

branches.read (Tenant)

branches.create (Tenant)

branches.update (Tenant)

branches.archive (Tenant)

branches.delete (Tenant)

Classes

classes.read (Tenant)

classes.create (Tenant)

classes.update (Tenant)

classes.archive (Tenant)

classes.delete (Tenant)

classes.assignCoach (Tenant)

classes.enrollStudents (Tenant)

classes.unenrollStudents (Tenant)

Coaches

coaches.read (Tenant)

coaches.create (Tenant)

coaches.update (Tenant)

coaches.archive (Tenant)

coaches.delete (Tenant)

coaches.assignClass (Tenant)

Documents

documents.read (Tenant)

documents.upload (Tenant)

documents.delete (Tenant)

documents.definitions.manage (Tenant)

Payments

payments.read (Tenant)

payments.createPlan (Tenant)

payments.updatePlan (Tenant)

payments.recordPayment (Tenant)

payments.adjust (Tenant)

payments.reports.read (Tenant)

payments.export (Tenant)

Profiles

profile.read.self (Self)

profile.update.self (Self)

profile.password.change.self (Self)

Reports

reports.read (Tenant)

reports.export (Tenant)

Settings

tenant.settings.manage (Tenant)

integrations.manage (Tenant)

Students

students.read (Tenant)

students.create (Tenant)

students.update (Tenant)

students.archive (Tenant)

students.delete (Tenant)

students.assignClass (Tenant)

students.changeClass (Tenant)

students.attendance.read (Tenant)

students.payments.read (Tenant)

students.documents.read (Tenant)

students.documents.manage (Tenant)

Users

users.read (Tenant)

users.create (Tenant)

users.update (Tenant)

users.disable (Tenant)

users.delete (Tenant)

✅ Admin template’de OLMAYACAK (SuperAdmin verir):

permissions.manage

users.protectAdmin

audit.read.tenant (istersen “Top Admin”e ayrıca verilecek)

audit.read.all

tenants.*

2) Coach (Tenant rol şablonu)

Scope: OwnClasses (kendi sınıflarıyla sınırlı)

Announcements

announcements.read.public (Tenant)

announcements.read (Tenant)

Attendance

attendance.read (OwnClasses)

attendance.take (OwnClasses)

attendance.edit (OwnClasses)

Classes

classes.read (OwnClasses)

classes.update (OwnClasses) (sınıf düzenleme yetkisi istiyorsan; istemezsen çıkarırız)

Students

students.read (OwnClasses)

students.attendance.read (OwnClasses)

Documents

documents.read (OwnClasses)

documents.upload (OwnClasses)

Profiles

profile.read.self (Self)

profile.update.self (Self)

profile.password.change.self (Self)

🚫 Coach template’de default OLMAYACAK (kulübe göre açılır / user override ile verilir):

payments.read (OwnClasses) (bazı kulüpler istiyor, bazıları istemiyor)

students.assignClass / classes.enrollStudents (default kapalı; senin örneğin tam bu)

students.update (öğrenci bilgisi düzenleme istiyorsan açılır)

3) Finance (Tenant rol şablonu)

Scope: Tenant (finans kulüp geneli)

Announcements

announcements.read.public (Tenant)

announcements.read (Tenant)

Payments

payments.read (Tenant)

payments.createPlan (Tenant)

payments.updatePlan (Tenant)

payments.recordPayment (Tenant)

payments.reports.read (Tenant)

payments.export (Tenant)

(opsiyonel) payments.adjust (Tenant) → default kapalı bırakmanı öneririm (iade/iptal kritik)

Students

students.read (Tenant) (finans öğrenci listesini görsün)

students.payments.read (Tenant)

Reports

reports.read (Tenant)

reports.export (Tenant)

Profiles

profile.read.self (Self)

profile.update.self (Self)

profile.password.change.self (Self)

🚫 Finance template’de default yok:

Attendance / Classes / Coaches yönetimi

Settings / Integrations

4) Student (Tenant rol şablonu)

Scope: Self (kendi verisi)

Announcements

announcements.read.public (Tenant)

announcements.read (Tenant)

Students

students.read (Self)

students.attendance.read (Self)

students.payments.read (Self)

students.documents.read (Self)

students.documents.manage (Self)

Documents

documents.read (Self)

documents.upload (Self)

Profiles

profile.read.self (Self)

profile.update.self (Self)

profile.password.change.self (Self)

5) SuperAdmin (Host – ayrı mantık)

SuperAdmin için TenantRolePermission seed basmak zorunda değilsin. SuperAdmin’in “host-level” yetkileri ayrı:

tenants.read (AllTenants)

tenants.manage (AllTenants)

tenants.switch (AllTenants)

permissions.manage (AllTenants)

users.protectAdmin (AllTenants)

audit.read.all (AllTenants)

permissions.explain (AllTenants)


Güncellenmiş Default Role Template (Seed) özeti

Aşağıdaki değişiklikler önceki template listelerine “revize” olarak uygulanacak.

A) Coach template revizyonu

Coach (OwnClasses) içinde artık:

classes.update ✅ açık (OwnClasses)

Diğerleri aynı kalır:

Attendance: read/take/edit (OwnClasses)

Students: read + attendance.read (OwnClasses)

Documents: read/upload (OwnClasses)

Announcements: read.public + read (Tenant)

Profiles: self

B) SuperAdmin template revizyonu

Artık SuperAdmin için:

Tüm PermissionNames (tam liste) verilecek

Scope stratejisi (SSOT):

Tenant içi modüller: Tenant scope (switch yaptığın tenant’ta full)

Host modüller: AllTenants scope

Pratik seed yaklaşımı (kod detayına girmeden SSOT kararı):

Resolver’da if SuperAdmin => PermissionNames.All dön.
Böylece “her tenant için ayrıca seed” zorunluluğu kalmaz.

permissions.manage yalnız SuperAdmin’in “verdiği” kritik yetki (istersen SuperAdmin’de doğal olarak var).

users.protectAdmin yalnız SuperAdmin.

“Tenant verisi içeren tüm modüller CurrentTenantId gerektirir ve global filter ile izole edilir; tenant seçimi/switch olmadan çalışmaz. Host-level yönetim ekranları tenant-scoped değildir.”

### 3.4 Tenant Context (CurrentTenantId) Zorunluluğu

Bu projede **tenant verisi içeren tüm modüller** (Students/Classes/Attendance/Payments/Documents/...) çalışmak için `CurrentTenantId` ister.

- `CurrentTenantId` yoksa (null/boş):
  - Tenant-scoped modül endpoint’leri **reddedilir** (403/400) veya kullanıcı **tenant seçme ekranına yönlendirilir** (MVC).
- `CurrentTenantId` varsa:
  - EF Core Global Query Filter otomatik uygulanır ve tenant izolasyonu garanti edilir.

**Kural:** Uygulama modüllerinde handler/service seviyesinde `TenantId.HasValue` kontrolü yapılmaz. Bu kontrol tek yerde (tenant context enforcement) yapılır.

### 3.5 Host-level İşlemler (Tenant-scoped olmayan ekranlar)

Bazı işlemler tenant bağlamı gerektirmez; bunlar "host-level" kabul edilir:
- Tenant yönetimi: `tenants.read`, `tenants.manage` (tenant oluştur/sil, plan/limit vb.)
- Tenant switch: `tenants.switch`
- SuperAdmin governance: `users.protectAdmin`, `permissions.manage`
- Audit global okuma: `audit.read.all`

Bu işlemler:
- TenantScopedEntity global filter kapsamına girmez (ör. Tenant tablosu).
- TenantScopedEntity üzerinde tüm tenant’ları görmek gerekiyorsa (örn. AuditEvent):
  - **İlgili sorgular `IgnoreQueryFilters()` ile** çalışır.
  - Sonrasında explicit filtre uygulanır (örn. seçilen tenant’a göre veya tüm tenantlar).


  ### 7.3 SuperAdmin Yetkisi ve Tenant Davranışı

- SuperAdmin **tüm permission'lara** sahiptir (tam kontrol).
- Ancak tenant verisi içeren modüller **tenant context gerektirir**:
  - SuperAdmin, uygulama modüllerini kullanmak için bir tenant seçmek zorundadır (switch).
  - Global query filter sebebiyle tenant seçmeden tenant verisi görülemez.

Host-level ekranlar tenant context gerektirmez:
- `tenants.*`, `permissions.manage`, `users.protectAdmin`, `audit.read.all` gibi.


### 5.5 Tenant Context Enforcement Standardı

Tenant-scoped endpoint’ler çalışmadan önce `CurrentTenantId` varlığı merkezi olarak doğrulanır.
- API: middleware/filter ile request başında kontrol
- MVC: action filter ile kontrol (gerekirse tenant seçme sayfasına yönlendirme)

Login / tenant listesi / tenant switch gibi endpoint’ler bu kontrolden muaftır.


- Tenant-scoped modüller için tenant context enforcement gerekir (middleware/filter).
- Host-level ekranlar tenant context gerektirmez; bazı sorgular IgnoreQueryFilters kullanabilir (örn. audit.read.all).

### 3.x Tenant-owned Entity Listesi (TenantScopedEntity)

Aşağıdaki entity’ler tenant verisi taşır ve `TenantScopedEntity` olmalıdır (TenantId zorunlu, global query filter uygulanır):

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
- ApplicationUser: TenantId taşır ve tenant-scoped kabul edilir (Identity entity olduğu için TenantScopedEntity’den türemez; buna ayrıca query filter uygulanır veya tenant context zaten claim’den gelir).

Host-level (TenantScopedEntity değildir):
- Tenant

Auth-level / özel:
- RefreshTokenEntity: tenant akışına göre TenantId zorunlu veya opsiyonel olabilir (login/refresh/switch tasarımına göre).

> Not: Mevcut projede TenantId şu entity’lerde henüz yok (dolaylı tenant var). SSOT gereği bu entity’lere TenantId eklenecek:
> Attendance, ClassEnrollment, ClassSession, Document, ProgressRecord, ProgressRecordValue

#### Tenant-scoped (tenant context zorunlu) — API Controllers

Tenant context zorunlu (CurrentTenantId yoksa reject):
- AdminsController
- AnnouncementsController
- AttendancesController
- BranchesController
- ClassesController
- ClassSessionsController
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

TenantsController içinde tenant-scoped endpoint:
- GET api/tenants/current-theme
- PUT api/tenants/current-theme

#### Exempt / Host-level — API Controllers & Actions

Tenant context zorunlu değildir (global filter kapsamı yok veya explicit yönetilir):
- AuthController: login/refresh/logout/password set/forgot (public/auth akışı)
- TenantsController host-level:
  - GET api/tenants
  - GET api/tenants/{id}
  - POST api/tenants
  - PUT api/tenants/{id}
  - DELETE api/tenants/{id}

Host-level sorgularda (ör. audit.read.all) tenant-scoped entity’ler için gerekiyorsa `IgnoreQueryFilters()` kullanılır.

#### Tenant-scoped (tenant context zorunlu) — WebMVC Controllers

Tenant context zorunlu (tenant seçimi yapılmadıysa yönlendirme/403):
- AdminDashboardController
- AdminsController
- AnnouncementsController
- AttendanceController
- BranchesController
- CalendarController
- ClassesController
- ClassSessionsController
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
- SuperAdminDashboardController (tenant içi ekranları varsa)

Exempt (tenant context zorunlu değil):
- AccountController (login/logout/forgot/set password)
- TenantsController (SuperAdmin tenant yönetimi ekranı)
- HomeController (eğer public/landing gibi kullanılıyorsa)

### Permission Ayrımı: students.assignClass vs classes.enrollStudents

- `students.assignClass`:
  Öğrencinin sınıf atamasını/değişikliğini yapar (domain state change).
  Örn: Student.ClassId / enrollment ilişkisi güncellenir.

- `classes.enrollStudents`:
  Sınıf üyeliğinin operasyonel yan etkilerini yönetir:
  - Öğrenciyi gelecekteki yoklama listelerine dahil eder
  - Sınıftan çıkarılan öğrenciyi gelecekteki yoklamalardan çıkarır
  - Geçmiş yoklamalar korunur (history bozulmaz)

Bu iki permission ayrı kalır. Bazı kulüplerde “atamayı yapan” ile “geleceğe yansıtma operasyonunu yapan” ayrıştırılabilir.

## 4.x Default Tenant Role Templates (Seed)

Scope varsayılanları:
- Admin: Tenant
- Finance: Tenant
- Coach: OwnClasses
- Student: Self
- SuperAdmin: tüm permission’lar (host dahil), tenant verisi için tenant context gerekir.

### Admin (Default)
Admin default olarak tenant içi tüm iş modüllerini yönetebilir.
- Admin default set = (Tenant içi permission’ların tamamı)
- Hariç / superadmin-only governance:
  - permissions.manage (sadece SuperAdmin grant/revoke)
  - users.protectAdmin (sadece SuperAdmin)
  - audit.read.all (sadece SuperAdmin)
  - tenants.manage / tenants.switch / tenants.read (host-level, sadece SuperAdmin)

Audit tenant okuma (audit.read.tenant) default admin’e verilmek zorunda değil; “Top Admin” olarak işaretlenen kullanıcıya ayrıca verilebilir.

### Coach (Default)
Coach default set (scope=OwnClasses):
- announcements.read.public, announcements.read (Tenant)
- classes.read, classes.update (OwnClasses)  ✅ (karar: update default açık)
- attendance.read, attendance.take, attendance.edit (OwnClasses)
- students.read, students.attendance.read (OwnClasses)
- documents.read, documents.upload (OwnClasses)
- profile.*.self (Self)

Kulübe göre opsiyonel (default kapalı, user override ile verilebilir):
- payments.read (OwnClasses)
- students.assignClass / classes.enrollStudents
- students.update

### Finance (Default)
Finance default set (scope=Tenant):
- announcements.read.public, announcements.read (Tenant)
- payments.read, payments.createPlan, payments.updatePlan, payments.recordPayment, payments.reports.read, payments.export (Tenant)
- (opsiyonel) payments.adjust (Tenant) — default kapalı önerilir
- students.read, students.payments.read (Tenant)
- reports.read, reports.export (Tenant)
- profile.*.self (Self)

### Student (Default)
Student default set (scope=Self):
- announcements.read.public, announcements.read (Tenant)
- students.read, students.attendance.read, students.payments.read (Self)
- students.documents.read, students.documents.manage (Self)
- documents.read, documents.upload (Self)
- profile.*.self (Self)

### SuperAdmin (Default)
SuperAdmin tüm permission’lara sahiptir (PermissionNames.All).
- Tenant verisi içeren modüller tenant context (CurrentTenantId) gerektirir.
- Host-level permission’lar: tenants.*, permissions.manage, users.protectAdmin, audit.read.all vb.

### Governance Rules (SSOT)

1) Protected Admin (SuperAdmin provisioned)
- SuperAdmin, tenant içindeki bir admini “Protected” olarak işaretleyebilir.
- Protected admin:
  - Tenant içindeki diğer adminler tarafından silinemez
  - Yetkileri/rolü değiştirilemez
  - Profil kritik alanları değiştirilemez
- Bu işlemler sadece SuperAdmin tarafından yapılabilir.

2) permissions.manage
- permissions.manage yetkisi tenant adminlerine sadece SuperAdmin tarafından verilebilir/geri alınabilir.
- Bu yetkiye sahip admin:
  - TenantRolePermission ve TenantUserPermissionOverride yönetebilir.

