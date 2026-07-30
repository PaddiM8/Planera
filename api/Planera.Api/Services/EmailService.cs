using ErrorOr;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

namespace Planera.Api.Services;

public class EmailService(IConfiguration configuration)
{
    private readonly IConfiguration _configuration = configuration;

    public async Task<ErrorOr<Success>> SendAsync(string subject, string htmlBody, string receiver)
    {
        if (string.IsNullOrEmpty(_configuration["Smtp:Host"]))
            return Error.Conflict("NotSupported", "The server is not equipped to send emails.");

        var email = new MimeMessage();
        email.From.Add(new MailboxAddress("Planera", _configuration["Smtp:Sender"]!));
        email.To.Add(MailboxAddress.Parse(receiver));
        email.Subject = subject;
        email.Body = new TextPart(TextFormat.Html)
        {
            Text = GenerateBody(subject, htmlBody),
        };

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(
            _configuration["Smtp:Host"]!,
            _configuration.GetValue<int>("Smtp:Port"),
            SecureSocketOptions.StartTls
        );
        await smtp.AuthenticateAsync(_configuration["Smtp:User"]!, _configuration["Smtp:Password"]!);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);

        return new ErrorOr<Success>();
    }

    private string GenerateBody(string title, string bodyHtml)
    {
        var frontendUrl = _configuration["FrontendUrl"];

        return $"""
            <!doctype html>
            <html>
            <head>
                <meta name="viewport" content="width=device-width" />
                <meta http-equiv="Content-Type" content="text/html; charset=UTF-8" />
                <title>Planera</title>
                <link rel="preconnect" href="https://rsms.me/">
                <link rel="stylesheet" href="https://rsms.me/inter/inter.css">
            </head>
            <body>
                <img class="logo" src="{frontendUrl}/logo-with-title-light.png" width="150" style="background-color: white; padding: 12px; border-radius: 8px">
                <div style="padding-left: 12px">
                    <h2 style="margin-top: 0; margin-bottom: -8px">{title}</h2>
                    {bodyHtml}
                    <p>- <a href="{frontendUrl}" style="color: cornflowerblue">Planera</a></p>
                </div>
            </body>
            </html>
            """;
    }
}