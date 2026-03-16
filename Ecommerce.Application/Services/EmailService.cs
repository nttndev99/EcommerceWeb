using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Interfaces.Services;

namespace Ecommerce.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration   _config;
    private readonly IHttpContextAccessor _http;

    public EmailService(IConfiguration config, IHttpContextAccessor http)
    {
        _config = config;
        _http   = http;
    }

    private string BaseUrl
    {
        get
        {
            var req = _http.HttpContext?.Request;
            return req is null ? "" : $"{req.Scheme}://{req.Host}";
        }
    }

    public async Task SendEmailConfirmationAsync(
        string toEmail, string fullName, string userId, string token)
    {
        var link = $"{BaseUrl}/Auth/ConfirmEmail?userId={userId}&token={token}";
        var body = $"""
            <h2>Confirm your email</h2>
            <p>Hi <strong>{fullName}</strong>,</p>
            <p>Thank you for registering. Please confirm your email address:</p>
            <p>
                <a href="{link}"
                    style="display:inline-block;padding:10px 24px;background:#111;color:#fff;
                    border-radius:6px;text-decoration:none;font-weight:600">
                    Confirm Email
                </a>
            </p>
            <p style="color:#888;font-size:.85rem">
                If you did not create an account, please ignore this email.
            </p>
            """;

        await SendAsync(toEmail, "Confirm your email address", body);
    }

    public async Task SendPasswordResetAsync(
        string toEmail, string fullName, string userId, string token)
    {
        var link = $"{BaseUrl}/Auth/ResetPassword?userId={userId}&token={token}";
        var body = $"""
            <h2>Reset your password</h2>
            <p>Hi <strong>{fullName}</strong>,</p>
            <p>We received a request to reset your password. Click below to proceed:</p>
            <p>
                <a href="{link}"
                   style="display:inline-block;padding:10px 24px;background:#111;color:#fff;
                          border-radius:6px;text-decoration:none;font-weight:600">
                    Reset Password
                </a>
            </p>
            <p style="color:#888;font-size:.85rem">
                This link expires in 1 hour. If you did not request this, ignore this email.
            </p>
            """;

        await SendAsync(toEmail, "Reset your password", body);
    }

    private async Task SendAsync(string to, string subject, string htmlBody)
    {
        var smtp = _config.GetSection("Smtp");

        using var client = new SmtpClient(smtp["Host"], int.Parse(smtp["Port"]!))
        {
            EnableSsl   = true,
            Credentials = new NetworkCredential(smtp["Username"], smtp["Password"]),
        };

        var mail = new MailMessage
        {
            From       = new MailAddress(smtp["From"]!, smtp["DisplayName"] ?? "Ecommerce"),
            Subject    = subject,
            Body       = htmlBody,
            IsBodyHtml = true,
        };
        mail.To.Add(to);

        await client.SendMailAsync(mail);
    }
}