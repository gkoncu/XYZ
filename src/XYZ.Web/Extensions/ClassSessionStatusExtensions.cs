using XYZ.Domain.Enums;

namespace XYZ.Web.Extensions
{
    public static class ClassSessionStatusExtensions
    {
        public static string ToDisplayName(this SessionStatus status)
        {
            return status switch
            {
                SessionStatus.Scheduled => "Planlandı",
                SessionStatus.Completed => "Tamamlandı",
                SessionStatus.Cancelled => "İptal Edildi",
                _ => "Bilinmiyor"
            };
        }

        public static string ToBadgeClass(this SessionStatus status)
        {
            return status switch
            {
                SessionStatus.Scheduled => "badge-soft badge-soft-warning",
                SessionStatus.Completed => "badge-soft badge-soft-success",
                SessionStatus.Cancelled => "badge-soft badge-soft-danger",
                _ => "badge-soft badge-soft-muted"
            };
        }
    }
}
