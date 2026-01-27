using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Features.Email.Options;

namespace XYZ.API.Services.Email;

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly IOptions<EmailOptions> _options;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<EmailOptions> options, ILogger<SmtpEmailSender> logger)
    {
        _options = options;
        _logger = logger;
    }

    public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default)
    {
        var opts = _options.Value;
        if (!opts.Enabled)
            return;

        if (string.IsNullOrWhiteSpace(toEmail))
            throw new ArgumentException("toEmail zorunludur.", nameof(toEmail));

        var smtp = opts.Smtp;

        if (string.IsNullOrWhiteSpace(smtp.Host))
            throw new InvalidOperationException("Email:Smtp:Host boş olamaz.");

        using var message = new MailMessage
        {
            From = new MailAddress(opts.FromAddress, opts.FromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        message.To.Add(new MailAddress(toEmail));

        using var client = new SmtpClient(smtp.Host, smtp.Port)
        {
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Credentials = new NetworkCredential(smtp.Username, smtp.Password),
            Timeout = Math.Max(1, smtp.TimeoutSeconds) * 1000,

            EnableSsl = smtp.UseSsl || smtp.UseStartTls
        };

        ct.ThrowIfCancellationRequested();

        try
        {
            await client.SendMailAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SMTP e-posta gonderimi basarisiz. To={ToEmail}", toEmail);
            throw;
        }
    }
}
