using System.Net;
using System.Net.Mail;

namespace SertecDashboard.Api.Services;

/// <summary>
/// Optional SMTP email service used for:
///   • Password-reset notifications sent to the admin
///   • Missed-question alerts sent to shift managers
///
/// To enable: fill in the Smtp section in appsettings.json / appsettings.Production.json.
/// If the host is empty the service silently skips sending (safe for dev/demo use).
/// </summary>
public class EmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService>  _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    /// <summary>
    /// Sends a plain-text email.
    /// Swallows all exceptions so a broken SMTP config never crashes the app.
    /// </summary>
    public async Task SendAsync(string to, string subject, string body)
    {
        if (string.IsNullOrWhiteSpace(to))
        {
            _logger.LogDebug("[EMAIL - no recipient] {Subject}", subject);
            return;
        }

        var host = _config["Smtp:Host"] ?? "";
        var user = _config["Smtp:User"] ?? "";

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(user))
        {
            _logger.LogDebug("[EMAIL - SMTP not configured] To:{To} | {Subject}", to, subject);
            return;
        }

        try
        {
            var port     = int.Parse(_config["Smtp:Port"] ?? "587");
            var pass     = _config["Smtp:Pass"] ?? "";
            var fromAddr = _config["Smtp:From"] ?? user;
            var ssl      = port == 465;

            using var client = new SmtpClient(host, port)
            {
                EnableSsl   = ssl,
                Credentials = new NetworkCredential(user, pass),
            };

            using var msg = new MailMessage(fromAddr, to, subject, body);
            await client.SendMailAsync(msg);
            _logger.LogInformation("[EMAIL] Sent to {To}: {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[EMAIL] Failed to send to {To}: {Subject}", to, subject);
        }
    }
}
