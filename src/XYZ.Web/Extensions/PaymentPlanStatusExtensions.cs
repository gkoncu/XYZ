using XYZ.Domain.Enums;

namespace XYZ.Web.Extensions
{
    public static class PaymentPlanStatusExtensions
    {
        public static string ToDisplayName(this PaymentPlanStatus status)
        {
            return status switch
            {
                PaymentPlanStatus.Active => "Aktif",
                PaymentPlanStatus.Archived => "Arşiv",
                PaymentPlanStatus.Cancelled => "İptal",
                _ => status.ToString()
            };
        }
    }
}
