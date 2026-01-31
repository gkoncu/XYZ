using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Domain.Enums
{
    public enum PaymentStatus
    {
        [Display(Name = "Beklemede")]
        Pending = 0,
        [Display(Name = "Ödendi")]
        Paid = 1,
        [Display(Name = "İptal Edildi")]
        Cancelled = 2,
        [Display(Name = "Gecikmiş")]
        Overdue = 3
    }
}
