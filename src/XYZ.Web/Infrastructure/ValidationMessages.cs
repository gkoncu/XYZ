namespace XYZ.Web.Infrastructure
{
    public static class ValidationMessages
    {
        // DataAnnotations placeholders:
        // {0} -> Display(Name)
        // {1} -> max
        // {2} -> min

        public const string Required = "{0} alanı zorunludur.";
        public const string MaxLength = "{0} en fazla {1} karakter olmalıdır.";

        public const string Email = "E-posta formatı geçersiz.";
        public const string Phone = "Telefon formatı geçersiz.";

        public const string TcIdentity = "T.C. Kimlik No 11 haneli ve sadece rakam olmalıdır.";

        public const string ExpiryBeforePublish = "Bitiş tarihi yayın tarihinden önce olamaz.";
        public const string PublishDateRequired = "Yayın tarihi alanı zorunludur.";
    }
}
