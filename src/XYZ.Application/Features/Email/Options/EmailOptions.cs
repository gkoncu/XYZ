namespace XYZ.Application.Features.Email.Options;

public sealed class EmailOptions
{
    public bool Enabled { get; set; } = false;

    public string FromAddress { get; set; } = "noreply@xyz.local";
    public string FromName { get; set; } = "XYZ";

    public SmtpOptions Smtp { get; set; } = new();

    public sealed class SmtpOptions
    {
        public string Host { get; set; } = "smtp.example.com";
        public int Port { get; set; } = 587;

        public string Username { get; set; } = "xxx";
        public string Password { get; set; } = "xxx";

        public bool UseStartTls { get; set; } = true;

        public bool UseSsl { get; set; } = false;

        public int TimeoutSeconds { get; set; } = 20;
    }
}
