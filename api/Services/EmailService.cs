using ErrorOr;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Planera.Services;

public class EmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<ErrorOr<Success>> SendAsync(string subject, TextPart body, string receiver)
    {
        if (string.IsNullOrEmpty(_configuration["Smtp:Host"]))
            return Error.Conflict("NotSupported", "The server is not equipped to send emails.");

        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse(_configuration["Smtp:Sender"]));
        email.To.Add(MailboxAddress.Parse(receiver));
        email.Subject = subject;
        email.Body = body;

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(
            _configuration["Smtp:Host"],
            _configuration.GetValue<int>("Smtp:Port"),
            SecureSocketOptions.StartTls
        );
        await smtp.AuthenticateAsync(_configuration["Smtp:User"], _configuration["Smtp:Password"]);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);

        return new ErrorOr<Success>();
    }
}