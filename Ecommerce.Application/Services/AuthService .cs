using Microsoft.AspNetCore.Identity;
using System.Text;
using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Auth;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Application.Interfaces.Services;
using Microsoft.AspNetCore.WebUtilities;

namespace Ecommerce.Infrastructure.Identity;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser>   _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IEmailService          _emailService;
    public AuthService(
        UserManager<AppUser>   userManager,
        SignInManager<AppUser> signInManager,
        IEmailService          emailService)
    {
        _userManager   = userManager;
        _signInManager = signInManager;
        _emailService  = emailService;
    }

    // ─────────────────────────────────────────────
    // REGISTER
    // ─────────────────────────────────────────────
    public async Task<Result> RegisterAsync(RegisterDto dto, CancellationToken ct = default)
    {
        var existing = await _userManager.FindByEmailAsync(dto.Email);
        if (existing is not null)
            return Result.Failure("Email is already registered.");

        var user = new AppUser
        {
            FullName    = dto.FullName,
            UserName    = dto.Email,
            Email       = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            CreatedAt   = DateTime.UtcNow,
            IsActive    = true,
        };

        var createResult = await _userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
            return Result.Failure(string.Join(", ", createResult.Errors.Select(e => e.Description)));

        // Assign Customer role
        await _userManager.AddToRoleAsync(user, "Customer");

        // Send confirmation email
        await SendConfirmationEmailAsync(user);

        return Result.Success();
    }

    // ─────────────────────────────────────────────
    // LOGIN
    // ─────────────────────────────────────────────
    public async Task<Result> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
            return Result.Failure("Invalid email or password.");

        if (!user.IsActive)
            return Result.Failure("Your account has been deactivated. Please contact support.");

        if (!user.EmailConfirmed)
            return Result.Failure("Please confirm your email before logging in.");

        var result = await _signInManager.PasswordSignInAsync(
            user, dto.Password, dto.RememberMe, lockoutOnFailure: true);

        if (result.IsLockedOut)
            return Result.Failure("Account is locked. Please try again later.");

        if (!result.Succeeded)
            return Result.Failure("Invalid email or password.");

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return Result.Success();
    }

    // ─────────────────────────────────────────────
    // LOGOUT
    // ─────────────────────────────────────────────
    public async Task LogoutAsync()
        => await _signInManager.SignOutAsync();

    // ─────────────────────────────────────────────
    // FORGOT PASSWORD
    // ─────────────────────────────────────────────
    public async Task<Result> ForgotPasswordAsync(ForgotPasswordDto dto, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);

        // Always return success to prevent email enumeration
        if (user is null || !user.EmailConfirmed)
            return Result.Success();

        var token      = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        await _emailService.SendPasswordResetAsync(
            user.Email!,
            user.FullName,
            user.Id,
            encodedToken);

        return Result.Success();
    }

    // ─────────────────────────────────────────────
    // RESET PASSWORD
    // ─────────────────────────────────────────────
    public async Task<Result> ResetPasswordAsync(ResetPasswordDto dto, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(dto.UserId);
        if (user is null)
            return Result.Failure("Invalid reset link.");

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(dto.Token));
        var result = await _userManager.ResetPasswordAsync(user, decodedToken, dto.NewPassword);

        return result.Succeeded
            ? Result.Success()
            : Result.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    // ─────────────────────────────────────────────
    // CONFIRM EMAIL
    // ─────────────────────────────────────────────
    public async Task<Result> ConfirmEmailAsync(ConfirmEmailDto dto, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(dto.UserId);
        if (user is null)
            return Result.Failure("Invalid confirmation link.");

        if (user.EmailConfirmed)
            return Result.Success(); // already confirmed

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(dto.Token));
        var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

        return result.Succeeded
            ? Result.Success()
            : Result.Failure("Confirmation failed. The link may have expired.");
    }

    // ─────────────────────────────────────────────
    // RESEND CONFIRMATION
    // ─────────────────────────────────────────────
    public async Task<Result> ResendConfirmationEmailAsync(string email, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null || user.EmailConfirmed)
            return Result.Success(); // silent

        await SendConfirmationEmailAsync(user);
        return Result.Success();
    }

    // ─────────────────────────────────────────────
    // HELPER
    // ─────────────────────────────────────────────
    private async Task SendConfirmationEmailAsync(AppUser user)
    {
        try
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            await _emailService.SendEmailConfirmationAsync(
                user.Email!, user.FullName, user.Id, encodedToken);
        }
        catch (Exception ex)
        {
            // ← Swallow email errors — don't block registration
        }
    }
}