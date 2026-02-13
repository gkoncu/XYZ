using System.Reflection;

namespace XYZ.Domain.Constants;

public static class PermissionNames
{
    // -------------------------
    // Announcements (Duyurular)
    // -------------------------
    public static class Announcements
    {
        public const string ReadPublic = "announcements.read.public";
        public const string Read = "announcements.read";
        public const string Create = "announcements.create";
        public const string Update = "announcements.update";
        public const string Publish = "announcements.publish";
        public const string Delete = "announcements.delete";
    }

    // -------------------------
    // Attendance (Yoklama)
    // -------------------------
    public static class Attendance
    {
        public const string Read = "attendance.read";
        public const string Take = "attendance.take";
        public const string Edit = "attendance.edit";

        public const string ReportsRead = "attendance.reports.read";
        public const string Export = "attendance.export";
    }

    // -------------------------
    // Audit (Denetim Kayıtları)
    // -------------------------
    public static class Audit
    {
        public const string ReadTenant = "audit.read.tenant";
        public const string ReadAll = "audit.read.all"; // only SuperAdmin
    }

    // -------------------------
    // Branches (Branş/Şube)
    // -------------------------
    public static class Branches
    {
        public const string Read = "branches.read";
        public const string Create = "branches.create";
        public const string Update = "branches.update";
        public const string Archive = "branches.archive";
        public const string Delete = "branches.delete";
    }

    // -------------------------
    // Classes (Sınıflar)
    // -------------------------
    public static class Classes
    {
        public const string Read = "classes.read";
        public const string Create = "classes.create";
        public const string Update = "classes.update";
        public const string Archive = "classes.archive";
        public const string Delete = "classes.delete";

        public const string AssignCoach = "classes.assignCoach";
        public const string EnrollStudents = "classes.enrollStudents"; // bulk
        public const string UnenrollStudents = "classes.unenrollStudents"; // bulk opsiyonel
    }

    // -------------------------
    // Coaches (Koçlar / Çalışanlar)
    // -------------------------
    public static class Coaches
    {
        public const string Read = "coaches.read";
        public const string Create = "coaches.create";
        public const string Update = "coaches.update";
        public const string Archive = "coaches.archive";
        public const string Delete = "coaches.delete";

        public const string AssignClass = "coaches.assignClass"; // koçu sınıfa ata/çıkar
    }

    // -------------------------
    // Documents (Dokümanlar)
    // -------------------------
    public static class Documents
    {
        public const string Read = "documents.read";
        public const string Upload = "documents.upload";
        public const string Delete = "documents.delete";

        public const string DefinitionsManage = "documents.definitions.manage"; // tür/şablon
    }

    // -------------------------
    // Payments (Ödemeler / Aidatlar)
    // -------------------------
    public static class Payments
    {
        public const string Read = "payments.read";

        public const string CreatePlan = "payments.createPlan";
        public const string UpdatePlan = "payments.updatePlan";

        public const string RecordPayment = "payments.recordPayment"; // ödendi/ödeme kaydı
        public const string Adjust = "payments.adjust"; // iptal/iade/düzeltme gibi kritik işlemler

        public const string ReportsRead = "payments.reports.read";
        public const string Export = "payments.export";
    }

    // -------------------------
    // Progress Metrics (Gelişim metrik tanımları)
    // -------------------------
    public static class ProgressMetrics
    {
        public const string Read = "progress.metrics.read";
        public const string Create = "progress.metrics.create";
        public const string Update = "progress.metrics.update";
        public const string Delete = "progress.metrics.delete";
    }

    // -------------------------
    // Progress Records (Gelişim kayıtları)
    // -------------------------
    public static class ProgressRecords
    {
        public const string Read = "progress.records.read";
        public const string Create = "progress.records.create";
        public const string Update = "progress.records.update";
        public const string Delete = "progress.records.delete";
    }

    // -------------------------
    // Profiles (Kendi hesabı)
    // -------------------------
    public static class Profiles
    {
        public const string ReadSelf = "profile.read.self";
        public const string UpdateSelf = "profile.update.self";
        public const string ChangePasswordSelf = "profile.password.change.self";
    }

    // -------------------------
    // Reports (Genel raporlar)
    // -------------------------
    public static class Reports
    {
        public const string Read = "reports.read";
        public const string Export = "reports.export";
    }

    // -------------------------
    // Settings (Kulüp ayarları / entegrasyonlar)
    // -------------------------
    public static class Settings
    {
        public const string TenantSettingsManage = "tenant.settings.manage";
        public const string IntegrationsManage = "integrations.manage";
    }

    // -------------------------
    // Students (Öğrenciler)
    // -------------------------
    public static class Students
    {
        public const string Read = "students.read";
        public const string Create = "students.create";
        public const string Update = "students.update";
        public const string Archive = "students.archive";
        public const string Delete = "students.delete";

        public const string AssignClass = "students.assignClass";
        public const string ChangeClass = "students.changeClass";

        public const string AttendanceRead = "students.attendance.read";
        public const string PaymentsRead = "students.payments.read";

        public const string DocumentsRead = "students.documents.read";
        public const string DocumentsManage = "students.documents.manage";
    }

    // -------------------------
    // Tenants (Host / Kulüp yönetimi)
    // -------------------------
    public static class Tenants
    {
        public const string Manage = "tenants.manage";   // only SuperAdmin
        public const string Switch = "tenants.switch";   // only SuperAdmin
        public const string Read = "tenants.read";       // only SuperAdmin
    }

    // -------------------------
    // Users (Kimlik / kullanıcı yönetimi)
    // -------------------------
    public static class Users
    {
        public const string Read = "users.read";
        public const string Create = "users.create";
        public const string Update = "users.update";
        public const string Disable = "users.disable";
        public const string Delete = "users.delete";

        public const string ProtectAdmin = "users.protectAdmin"; // only SuperAdmin
    }

    // -------------------------
    // Permissions (Yetki yönetimi)
    // -------------------------
    public static class Permissions
    {
        public const string Manage = "permissions.manage"; // only SuperAdmin can grant/revoke
        public const string Explain = "permissions.explain";
    }

    // -------------------------
    // Helpers (Known list validation)
    // -------------------------
    public static readonly IReadOnlyCollection<string> All = BuildAll();
    public static readonly ISet<string> AllSet = new HashSet<string>(All, StringComparer.Ordinal);

    public static bool IsKnown(string permissionKey) => AllSet.Contains(permissionKey);

    private static IReadOnlyCollection<string> BuildAll()
    {
        var list = new List<string>();

        var nested = typeof(PermissionNames).GetNestedTypes(BindingFlags.Public);
        foreach (var t in nested)
        {
            foreach (var f in t.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
            {
                if (f.FieldType != typeof(string)) continue;

                var value = f.GetValue(null) as string;
                if (!string.IsNullOrWhiteSpace(value))
                    list.Add(value);
            }
        }

        return list
            .Distinct(StringComparer.Ordinal)
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToArray();
    }
}
