using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Interfaces.Services;

namespace Ecommerce.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration       _config;
    private readonly IHttpContextAccessor _http;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IConfiguration        config,
        IHttpContextAccessor  http,
        ILogger<EmailService> logger)
    {
        _config = config;
        _http   = http;
        _logger = logger;
    }

    private string BaseUrl
    {
        get
        {
            var req = _http.HttpContext?.Request;
            return req is null ? "" : $"{req.Scheme}://{req.Host}";
        }
    }

    // ─────────────────────────────────────────────
    // EMAIL CONFIRMATION
    // ─────────────────────────────────────────────
    public async Task SendEmailConfirmationAsync(
        string toEmail, string fullName, string userId, string token)
    {
        var link = $"{BaseUrl}/Admin/Auth/ConfirmEmail?userId={userId}&token={token}";

        var body = $"""
            <div style="font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',sans-serif;
                        max-width:560px;margin:0 auto;color:#111">
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
                    Or copy this link:<br>
                    <a href="{link}" style="color:#6366f1;word-break:break-all">{link}</a>
                </p>
                <p style="color:#888;font-size:.85rem">
                    If you did not create an account, please ignore this email.
                </p>
            </div>
            """;

        await SendAsync(toEmail, "Confirm your email address", body);
    }

    // ─────────────────────────────────────────────
    // PASSWORD RESET
    // ─────────────────────────────────────────────
    public async Task SendPasswordResetAsync(
        string toEmail, string fullName, string userId, string token)
    {
        var link = $"{BaseUrl}/Admin/Auth/ResetPassword?userId={userId}&token={token}";

        var body = $"""
            <div style="font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',sans-serif;
                        max-width:560px;margin:0 auto;color:#111">
                <h2>Reset your password</h2>
                <p>Hi <strong>{fullName}</strong>,</p>
                <p>We received a request to reset your password:</p>
                <p>
                    <a href="{link}"
                       style="display:inline-block;padding:10px 24px;background:#111;color:#fff;
                              border-radius:6px;text-decoration:none;font-weight:600">
                        Reset Password
                    </a>
                </p>
                <p style="color:#888;font-size:.85rem">
                    Or copy this link:<br>
                    <a href="{link}" style="color:#6366f1;word-break:break-all">{link}</a>
                </p>
                <p style="color:#888;font-size:.85rem">
                    This link expires in 1 hour.
                </p>
            </div>
            """;

        await SendAsync(toEmail, "Reset your password", body);
    }

    // ─────────────────────────────────────────────
    // ORDER CONFIRMATION
    // ─────────────────────────────────────────────
    public async Task SendOrderConfirmationAsync(
        string toEmail, string fullName, string orderCode, decimal total)
    {
        var orderLink = $"{BaseUrl}/Orders/Detail?orderCode={orderCode}";

        var body = $"""
            <div style="font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',sans-serif;
                        max-width:560px;margin:0 auto;color:#111">
                <h2>Order Confirmed ✓</h2>
                <p>Hi <strong>{fullName}</strong>, thank you for your purchase!</p>
                <table style="width:100%;border-collapse:collapse;margin:1.5rem 0">
                    <tr>
                        <td style="padding:10px 14px;border:1px solid #e5e7eb;background:#fafafa;font-weight:600;width:40%">Order Code</td>
                        <td style="padding:10px 14px;border:1px solid #e5e7eb;font-family:monospace">{orderCode}</td>
                    </tr>
                    <tr>
                        <td style="padding:10px 14px;border:1px solid #e5e7eb;background:#fafafa;font-weight:600">Total</td>
                        <td style="padding:10px 14px;border:1px solid #e5e7eb;font-weight:700;color:#16a34a">{total:N0} ₫</td>
                    </tr>
                </table>
                <p style="margin-top:1.5rem">
                    <a href="{orderLink}"
                       style="display:inline-block;padding:10px 24px;background:#111;color:#fff;
                              border-radius:6px;text-decoration:none;font-weight:600">
                        View Order
                    </a>
                </p>
            </div>
            """;

        await SendAsync(toEmail, $"Order Confirmed — {orderCode}", body);
    }

    // ─────────────────────────────────────────────
    // SHARED SEND
    // ─────────────────────────────────────────────
    private async Task SendAsync(string to, string subject, string htmlBody)
    {
        var smtp = _config.GetSection("Smtp");

        var host     = smtp["Host"];
        var username = smtp["Username"];
        var password = smtp["Password"];
        var from     = smtp["From"];
        var port     = int.TryParse(smtp["Port"], out var p) ? p : 587;

        // ── Validate config ──────────────────────
        if (string.IsNullOrWhiteSpace(host))
        {
            _logger.LogError("SMTP Host is not configured in appsettings.json");
            throw new InvalidOperationException("SMTP Host is not configured.");
        }
        if (string.IsNullOrWhiteSpace(from))
        {
            _logger.LogError("SMTP From is not configured in appsettings.json");
            throw new InvalidOperationException("SMTP From address is not configured.");
        }
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            _logger.LogError("SMTP Username or Password is not configured in appsettings.json");
            throw new InvalidOperationException("SMTP credentials are not configured.");
        }

        _logger.LogInformation("Sending email to {To} via {Host}:{Port}", to, host, port);

        try
        {
            using var client = new SmtpClient(host, port)
            {
                EnableSsl   = true,
                Credentials = new NetworkCredential(username, password),
                Timeout     = 10_000, // 10 seconds
            };

            var mail = new MailMessage
            {
                From       = new MailAddress(from, smtp["DisplayName"] ?? "Ecommerce"),
                Subject    = subject,
                Body       = htmlBody,
                IsBodyHtml = true,
            };
            mail.To.Add(to);

            await client.SendMailAsync(mail);

            _logger.LogInformation("Email sent successfully to {To}", to);
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex,
                "SMTP error sending to {To}. StatusCode: {Code}. " +
                "Check Host={Host}, Port={Port}, SSL=true, Username={User}",
                to, ex.StatusCode, host, port, username);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error sending email to {To}", to);
            throw;
        }
    }
}