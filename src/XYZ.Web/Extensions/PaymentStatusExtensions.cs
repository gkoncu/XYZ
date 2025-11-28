using XYZ.Domain.Enums;

namespace XYZ.Web.Extensions
{
    public static class PaymentStatusExtensions
    {
        public static string ToDisplayName(this PaymentStatus status)
            => status switch
            {
                PaymentStatus.Pending => "Beklemede",
                PaymentStatus.Paid => "Ödendi",
                PaymentStatus.Overdue => "Gecikmiş",
                PaymentStatus.Cancelled => "İptal",
                _ => status.ToString()
            };
    }
}
