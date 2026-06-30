using Application.Abstractions.Email;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Infrastructure.Email;

internal sealed class EmailService : IEmailService
{
    private IOptions<EmailOptions> EmailOptions;
    private SendGridClient _client;

    public EmailService(IOptions<EmailOptions> emailOptions)
    {
        EmailOptions = emailOptions;
        _client ??= new SendGridClient(emailOptions.Value.ApiKey);
    }

    public async Task SendAsync(string recipient, string subject, string body)
    {
        var msg = new SendGridMessage()
        {
            From = new EmailAddress(recipient, "Warning Zone"),
            Subject = subject,
            PlainTextContent = body,
        };

        await _client.SendEmailAsync(msg);
    }
}