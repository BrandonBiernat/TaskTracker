using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using Shared.Interfaces.Services;

namespace Services;

public class EmailService(IConfiguration configuration) : IEmailService
{
    public async Task SendAsync(
        string to, 
        string subject, 
        string htmlBody) {
        MimeMessage message = new();
        message.From.Add(MailboxAddress.Parse(configuration["Email:From"]!));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        using SmtpClient client = new();
        await client.ConnectAsync(
            host: configuration["Email:Host"],
            port: int.Parse(configuration["Email:Port"]!),
            options: MailKit.Security.SecureSocketOptions.None);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
