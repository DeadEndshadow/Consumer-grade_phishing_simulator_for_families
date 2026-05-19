#pragma warning disable SYSLIB0006 // SmtpClient obsolete — MailKit not yet a dependency
using System.Net;
using System.Net.Mail;

namespace PhishingSimulator.Web.Services;

public class SmtpEmailSender(IConfiguration config, ILogger<SmtpEmailSender> logger) : IEmailSender
{
    public async Task SendAsync(string toAddress, string toName, string subject, string htmlBody)
    {
        var host = config["Smtp:Host"];

        if (string.IsNullOrWhiteSpace(host))
        {
            // Dev fallback: extract href links then strip tags so URLs aren't lost
            var links = System.Text.RegularExpressions.Regex
                .Matches(htmlBody, @"href=""([^""]+)""")
                .Select(m => m.Groups[1].Value)
                .Where(u => u.StartsWith("http"))
                .Distinct()
                .ToList();

            var linkBlock = links.Count > 0
                ? "\n\n--- LINKS IN THIS EMAIL ---\n" + string.Join("\n", links)
                : "";

            logger.LogWarning(
                "[EMAIL — SMTP not configured]\nTo: {To}\nSubject: {Subject}{Links}",
                toAddress, subject, linkBlock);
            return;
        }

        var port       = int.Parse(config["Smtp:Port"] ?? "587");
        var enableSsl  = bool.Parse(config["Smtp:EnableSsl"] ?? "true");
        var username   = config["Smtp:Username"] ?? "";
        var password   = config["Smtp:Password"] ?? "";
        var fromAddr   = config["Smtp:FromAddress"] ?? username;
        var fromName   = config["Smtp:FromName"] ?? "PhishSim";

        using var client = new SmtpClient(host, port)
        {
            EnableSsl   = enableSsl,
            Credentials = new NetworkCredential(username, password),
        };

        using var message = new MailMessage
        {
            From       = new MailAddress(fromAddr, fromName),
            Subject    = subject,
            Body       = htmlBody,
            IsBodyHtml = true,
        };
        message.To.Add(new MailAddress(toAddress, toName));

        await client.SendMailAsync(message);
        logger.LogInformation("Email sent to {To}: {Subject}", toAddress, subject);
    }
}
#pragma warning restore SYSLIB0006
