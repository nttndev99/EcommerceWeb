using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ecommerce.Application.DTOs.Auth;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Interfaces.Services;

namespace Ecommerce.Web.Controllers;
[Area("Admin")]
public class AuthController : Controller
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // REGISTER
    [HttpGet]
    public IActionResult Register(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToLocal(returnUrl);
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterDto dto, string? returnUrl = null, CancellationToken ct = default)
    {
        ViewBag.ReturnUrl = returnUrl;
        if (!ModelState.IsValid) return View(dto);
        var result = await _authService.RegisterAsync(dto, ct);
        if (!result.IsSuccess) { ModelState.AddModelError(string.Empty, result.Error!); return View(dto); }
        return RedirectToAction(nameof(RegisterConfirmation), new { email = dto.Email });
    }

    [HttpGet]
    public IActionResult RegisterConfirmation(string email) { ViewBag.Email = email; return View(); }

    // LOGIN
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectAfterLogin(returnUrl);

        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto dto, string? returnUrl = null, CancellationToken ct = default)
    {
        ViewBag.ReturnUrl = returnUrl;

        if (!ModelState.IsValid)
            return View(dto);

        var result = await _authService.LoginAsync(dto, ct);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(dto);
        }

        return RedirectAfterLogin(returnUrl);
    }
    private IActionResult RedirectAfterLogin(string? returnUrl)
    {
        if (User.IsInRole("Admin"))
            return RedirectToAction("Index", "Home", new { area = "Admin" });

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Home", new { area = "" });
    }
    
    // LOGOUT
    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var isAdmin = User.IsInRole("Admin");

        await _authService.LogoutAsync();

        if (isAdmin)
            return RedirectToAction(nameof(Login));

        return RedirectToAction("Index", "Home", new { area = "" });
    }
    // CONFIRM EMAIL
    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string userId, string token, CancellationToken ct)
    {
        var result = await _authService.ConfirmEmailAsync(new ConfirmEmailDto { UserId = userId, Token = token }, ct);
        ViewBag.Success = result.IsSuccess; ViewBag.Error = result.Error;
        return View();
    }

    [HttpGet]  public IActionResult ResendConfirmation() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendConfirmation(string email, CancellationToken ct)
    {
        await _authService.ResendConfirmationEmailAsync(email, ct);
        ViewBag.Sent = true; ViewBag.Email = email;
        return View();
    }

    // FORGOT PASSWORD
    [HttpGet]  public IActionResult ForgotPassword() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(dto);
        await _authService.ForgotPasswordAsync(dto, ct);
        return RedirectToAction(nameof(ForgotPasswordConfirmation), new { email = dto.Email });
    }

    [HttpGet]
    public IActionResult ForgotPasswordConfirmation(string email) { ViewBag.Email = email; return View(); }

    // RESET PASSWORD
    [HttpGet]
    public IActionResult ResetPassword(string userId, string token)
        => View(new ResetPasswordDto { UserId = userId, Token = token });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(dto);
        var result = await _authService.ResetPasswordAsync(dto, ct);
        if (!result.IsSuccess) { ModelState.AddModelError(string.Empty, result.Error!); return View(dto); }
        return RedirectToAction(nameof(ResetPasswordConfirmation));
    }

    [HttpGet]  public IActionResult ResetPasswordConfirmation() => View();
    [HttpGet]  public IActionResult AccessDenied() => View();

    private IActionResult RedirectToLocal(string? returnUrl)
        => !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? Redirect(returnUrl)
            : RedirectToAction("Index", "Home");
}