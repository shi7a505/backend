using System.Net;
using System.Net.Mail;
using Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services;

public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _config;

    public SmtpEmailSender(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default)
    {
        var host = _config["Smtp:Host"] ?? "smtp.gmail.com";
        var port = int.TryParse(_config["Smtp:Port"], out var p) ? p : 587;

        var user = _config["Smtp:User"] ?? throw new InvalidOperationException("Smtp:User not configured");
        var pass = _config["Smtp:AppPassword"] ?? throw new InvalidOperationException("Smtp:AppPassword not configured");
        var from = _config["Smtp:From"] ?? user;

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,              // مهم
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Credentials = new NetworkCredential(user, pass)
        };

        using var msg = new MailMessage(from, toEmail, subject, htmlBody)
        {
            IsBodyHtml = true
        };

        await client.SendMailAsync(msg);
    }
}