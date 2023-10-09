namespace AssetManagement.Services;

public class MockEmailService : IEmailService
{
    public Task SendEmailAsync(string toEmail, string subject, string content)
    {
        // mock
        return Task.CompletedTask;
    }
}