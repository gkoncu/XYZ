namespace XYZ.Domain.Enums;

public enum PermissionScope : byte
{
    Self = 10,        // sadece kendi kaydı
    OwnClasses = 20,  // koç -> kendi sınıfları
    Branch = 30,      // branş bazlı
    Tenant = 40,      // tenant geneli
    AllTenants = 50   // host / tüm tenantlar
}
