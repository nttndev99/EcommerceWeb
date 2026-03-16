using Ecommerce.Application.DTOs.Admin;
using Ecommerce.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.MVC.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "RequireAdmin")]
public class UsersManagerController : Controller
{
    private readonly IUsersManagerService _usersService;

    public UsersManagerController(IUsersManagerService usersService)
    {
        _usersService = usersService;
    }

    // GET /Admin/UsersManager
    public async Task<IActionResult> Index(UserManagerFilterParams filter, CancellationToken ct)
    {
        var result = await _usersService.GetPagedAsync(filter, ct);
        ViewBag.Filter   = filter;
        ViewBag.AllRoles = await _usersService.GetAllRolesAsync(ct);
        return View(result);
    }

    // GET /Admin/UsersManager/Detail/{id}
    public async Task<IActionResult> Detail(string id, CancellationToken ct)
    {
        var user = await _usersService.GetByIdAsync(id, ct);
        if (user is null) return NotFound();
        ViewBag.AllRoles = await _usersService.GetAllRolesAsync(ct);
        return View(user);
    }

    // ─────────────────────────────────────────────
    // CREATE (gọi lại AuthService.RegisterAsync)
    // ─────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        ViewBag.AllRoles = await _usersService.GetAllRolesAsync(ct);
        return View(new AdminCreateUserDto());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminCreateUserDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.AllRoles = await _usersService.GetAllRolesAsync(ct);
            return View(dto);
        }

        var result = await _usersService.CreateAsync(dto, ct);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            ViewBag.AllRoles = await _usersService.GetAllRolesAsync(ct);
            return View(dto);
        }

        TempData["Success"] = $"User '{dto.Email}' created successfully.";
        return RedirectToAction(nameof(Index));
    }

    // ─────────────────────────────────────────────
    // EDIT
    // ─────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Edit(string id, CancellationToken ct)
    {
        var user = await _usersService.GetByIdAsync(id, ct);
        if (user is null) return NotFound();

        return View(new AdminEditUserDto
        {
            Id          = user.Id,
            FullName    = user.FullName,
            PhoneNumber = user.PhoneNumber,
            IsActive    = user.IsActive,
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AdminEditUserDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(dto);

        var result = await _usersService.UpdateAsync(dto, ct);
        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess
            ? "User updated successfully." : result.Error;

        return result.IsSuccess
            ? RedirectToAction(nameof(Detail), new { id = dto.Id })
            : View(dto);
    }

    // ─────────────────────────────────────────────
    // LOCK / UNLOCK
    // ─────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Lock(string id, CancellationToken ct)
    {
        var result = await _usersService.LockAsync(id, ct);
        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess
            ? "User locked successfully." : result.Error;
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Unlock(string id, CancellationToken ct)
    {
        var result = await _usersService.UnlockAsync(id, ct);
        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess
            ? "User unlocked successfully." : result.Error;
        return RedirectToAction(nameof(Detail), new { id });
    }

    // ─────────────────────────────────────────────
    // RESET PASSWORD
    // ─────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> ResetPassword(string id, CancellationToken ct)
    {
        var user = await _usersService.GetByIdAsync(id, ct);
        if (user is null) return NotFound();
        ViewBag.UserName = user.FullName;
        return View(new AdminResetPasswordDto { Id = id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(AdminResetPasswordDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(dto);

        var result = await _usersService.ResetPasswordAsync(dto, ct);
        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess
            ? "Password reset successfully." : result.Error;

        return result.IsSuccess
            ? RedirectToAction(nameof(Detail), new { id = dto.Id })
            : View(dto);
    }

    // ─────────────────────────────────────────────
    // DELETE
    // ─────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        var result = await _usersService.DeleteAsync(id, ct);
        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess
            ? "User deleted." : result.Error;
        return RedirectToAction(nameof(Index));
    }
}