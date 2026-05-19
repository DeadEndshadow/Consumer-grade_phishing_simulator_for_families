namespace PhishingSimulator.Web.Services;

public interface IEmailSender
{
    Task SendAsync(string toAddress, string toName, string subject, string htmlBody);
}
