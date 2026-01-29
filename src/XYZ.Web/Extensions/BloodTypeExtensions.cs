using XYZ.Domain.Enums;

namespace XYZ.Web.Extensions
{
    public static class BloodTypeExtensions
    {
        public static string ToDisplayName(this BloodType bloodType)
        {
            return bloodType switch
            {
                BloodType.A_Positive => "A+",
                BloodType.A_Negative => "A-",
                BloodType.B_Positive => "B+",
                BloodType.B_Negative => "B-",
                BloodType.AB_Positive => "AB+",
                BloodType.AB_Negative => "AB-",
                BloodType.O_Positive => "O+",
                BloodType.O_Negative => "O-",
                _ => "Bilinmiyor"
            };
        }
    }
}
