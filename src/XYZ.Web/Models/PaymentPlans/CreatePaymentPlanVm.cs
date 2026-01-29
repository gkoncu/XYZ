using System;
using System.ComponentModel.DataAnnotations;

namespace XYZ.Web.Models.PaymentPlans
{
    public class CreatePaymentPlanVm : IValidatableObject
    {
        [Range(1, int.MaxValue, ErrorMessage = "Öğrenci seçimi geçersiz.")]
        public int StudentId { get; set; }

        public string StudentFullName { get; set; } = "";

        [Range(typeof(decimal), "1", "99999", ErrorMessage = "Toplam tutar 0'dan büyük olmalıdır.")]
        public decimal TotalAmount { get; set; }

        [DataType(DataType.Date)]
        public DateTime FirstDueDate { get; set; }

        public bool IsInstallment { get; set; }

        [Range(1, 52, ErrorMessage = "Taksit sayısı 1 ile 52 arasında olmalıdır.")]
        public int TotalInstallments { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var today = DateTime.Today;

            if (TotalAmount <= 1)
                yield return new ValidationResult("Toplam tutar 0'dan büyük olmalıdır.", new[] { nameof(TotalAmount) });

            if (FirstDueDate.Date < today.AddDays(-365) || FirstDueDate.Date > today.AddDays(365))
                yield return new ValidationResult("İlk vade tarihi 1 yıl geçmişe/ileriye gidemez.", new[] { nameof(FirstDueDate) });

            if (!IsInstallment && TotalInstallments != 1)
                yield return new ValidationResult("Peşin planda taksit sayısı 1 olmalıdır.", new[] { nameof(TotalInstallments) });

            if (IsInstallment && (TotalInstallments < 2 || TotalInstallments > 52))
                yield return new ValidationResult("Taksitli planda taksit sayısı 2 ile 52 arasında olmalıdır.", new[] { nameof(TotalInstallments) });
        }
    }
}
