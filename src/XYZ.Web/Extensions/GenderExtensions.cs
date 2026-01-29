using System.Runtime.CompilerServices;
using XYZ.Domain.Enums;

namespace XYZ.Web.Extensions
{
    public static class GenderExtensions
    {
        public static string ToDisplayName(this Gender gender)
        {
            return gender switch
            {
                Gender.Male => "Erkek",
                Gender.Female => "Kadın",
                Gender.PreferNotToSay => "Belirtmek İstemiyorum",
                _ => "Bilinmiyor"

            };
        }
    }
}
