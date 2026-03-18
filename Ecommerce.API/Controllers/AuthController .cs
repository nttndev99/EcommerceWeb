using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Ecommerce.API.Extensions;
using Ecommerce.API.Models;
using Ecommerce.Application.DTOs.Auth;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Application.Interfaces.Services;

namespace Ecommerce.API.Controllers;

public class AuthController : BaseApiController
{
    private readonly IAuthService         _authService;
    private readonly ITokenService        _tokenService;
    private readonly UserManager<AppUser> _userManager;

    public AuthController(
        IAuthService         authService,
        ITokenService        tokenService,
        UserManager<AppUser> userManager)
    {
        _authService  = authService;
        _tokenService = tokenService;
        _userManager  = userManager;
    }

    /// <summary>Register a new customer account</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest("Validation failed.",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

        var result = await _authService.RegisterAsync(dto, ct);
        if (!result.IsSuccess)
            return BadRequest(result.Error!);

        return Ok(new { message = "Registration successful. Please check your email to confirm." },
            "Registration successful.");
    }

    /// <summary>Login and receive JWT token</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest("Validation failed.",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

        // Validate credentials via AuthService
        var loginResult = await _authService.LoginAsync(dto, ct);
        if (!loginResult.IsSuccess)
            return Unauthorized(loginResult.Error!);

        // Generate JWT
        var user  = await _userManager.FindByEmailAsync(dto.Email);
        var token = await _tokenService.GenerateTokenAsync(user!);

        return Ok(new
        {
            token,
            expiresIn  = 60 * 60, // seconds
            tokenType  = "Bearer",
            userId     = user!.Id,
            email      = user.Email,
            fullName   = user.FullName,
            roles      = await _userManager.GetRolesAsync(user),
        }, "Login successful.");
    }

    /// <summary>Confirm email address</summary>
    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(
        [FromQuery] string userId, [FromQuery] string token, CancellationToken ct)
    {
        var result = await _authService.ConfirmEmailAsync(
            new ConfirmEmailDto { UserId = userId, Token = token }, ct);

        return result.IsSuccess
            ? Ok(new { confirmed = true }, "Email confirmed successfully.")
            : BadRequest(result.Error!);
    }

    /// <summary>Request password reset link</summary>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordDto dto, CancellationToken ct)
    {
        await _authService.ForgotPasswordAsync(dto, ct);
        // Always 200 — prevent email enumeration
        return Ok(new { sent = true },
            "If that email is registered, a reset link has been sent.");
    }

    /// <summary>Reset password with token</summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordDto dto, CancellationToken ct)
    {
        var result = await _authService.ResetPasswordAsync(dto, ct);
        return result.IsSuccess
            ? Ok(new { reset = true }, "Password reset successfully.")
            : BadRequest(result.Error!);
    }

    /// <summary>Resend email confirmation</summary>
    [HttpPost("resend-confirmation")]
    public async Task<IActionResult> ResendConfirmation(
        [FromBody] string email, CancellationToken ct)
    {
        await _authService.ResendConfirmationEmailAsync(email, ct);
        return Ok(new { sent = true },
            "If that email is registered and unconfirmed, a new link has been sent.");
    }

    /// <summary>Get current user profile</summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var user = await _userManager.FindByIdAsync(CurrentUserId!);
        if (user is null) return NotFound("User not found.");

        var roles = await _userManager.GetRolesAsync(user);
        return Ok(new
        {
            user.Id,
            user.FullName,
            user.Email,
            user.PhoneNumber,
            user.IsActive,
            user.EmailConfirmed,
            user.CreatedAt,
            user.LastLoginAt,
            Roles = roles,
        });
    }
}