using SendGrid;
using SendGrid.Helpers.Mail;

namespace AssetManagement.Services;

public class SendGridEmailService : IEmailService
{
    private readonly string _apiKey;

    public SendGridEmailService(string apiKey)
    {
        _apiKey = apiKey;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string content)
    {
        var client = new SendGridClient(_apiKey);
        var from = new EmailAddress("your-email@example.com", "Your App Name");
        var to = new EmailAddress(toEmail);
        var msg = MailHelper.CreateSingleEmail(from, to, subject, content, content);
        await client.SendEmailAsync(msg);
    }
}