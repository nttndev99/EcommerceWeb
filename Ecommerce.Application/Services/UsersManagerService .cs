using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Admin;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Application.Interfaces.Services;

namespace Ecommerce.Infrastructure.Identity;

public class UsersManagerService : IUsersManagerService
{
    private readonly UserManager<AppUser>      _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    public UsersManagerService(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }
    public static class SystemAccounts
    {
        public const string AdminEmail = "admin@ecommerce.com";
    }
    // ─────────────────────────────────────────────
    // GET PAGED
    // ─────────────────────────────────────────────
    public async Task<PagedResult<AdminUserListDto>> GetPagedAsync(
        UserManagerFilterParams filter, CancellationToken ct = default)
    {
        var query = _userManager.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var kw = filter.Search.Trim().ToLower();
            query = query.Where(u =>
                u.FullName.ToLower().Contains(kw) ||
                (u.Email        != null && u.Email.ToLower().Contains(kw)) ||
                (u.PhoneNumber  != null && u.PhoneNumber.Contains(kw)));
        }

        if (filter.IsActive.HasValue)
            query = query.Where(u => u.IsActive == filter.IsActive.Value);

        if (filter.IsLockedOut.HasValue)
        {
            var now = DateTimeOffset.UtcNow;
            query = filter.IsLockedOut.Value
                ? query.Where(u => u.LockoutEnd != null && u.LockoutEnd > now)
                : query.Where(u => u.LockoutEnd == null || u.LockoutEnd <= now);
        }

        query = (filter.SortBy?.ToLower(), filter.SortDirection?.ToLower()) switch
        {
            ("fullname",  "asc") => query.OrderBy(u => u.FullName),
            ("fullname",  _)     => query.OrderByDescending(u => u.FullName),
            ("email",     "asc") => query.OrderBy(u => u.Email),
            ("email",     _)     => query.OrderByDescending(u => u.Email),
            ("lastlogin", "asc") => query.OrderBy(u => u.LastLoginAt),
            ("lastlogin", _)     => query.OrderByDescending(u => u.LastLoginAt),
            ("createdat", "asc") => query.OrderBy(u => u.CreatedAt),
            _                    => query.OrderByDescending(u => u.CreatedAt),
        };

        var totalCount = await query.CountAsync(ct);
        var users = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        // Filter by role if needed (in-memory because UserManager doesn't support IQueryable for roles)
        var result = new List<AdminUserListDto>();
        var now2 = DateTimeOffset.UtcNow;

        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);

            if (!string.IsNullOrWhiteSpace(filter.Role) && !roles.Contains(filter.Role))
                continue;

            result.Add(new AdminUserListDto
            {
                Id             = u.Id,
                FullName       = u.FullName,
                Email          = u.Email ?? string.Empty,
                PhoneNumber    = u.PhoneNumber,
                IsActive       = u.IsActive,
                EmailConfirmed = u.EmailConfirmed,
                IsLockedOut    = u.LockoutEnd.HasValue && u.LockoutEnd > now2,
                CreatedAt      = u.CreatedAt,
                LastLoginAt    = u.LastLoginAt,
                Roles          = roles.ToList(),
            });
        }

        return PagedResult<AdminUserListDto>.Create(result, totalCount, filter.PageNumber, filter.PageSize);
    }

    // ─────────────────────────────────────────────
    // GET BY ID
    // ─────────────────────────────────────────────
    public async Task<AdminUserDto?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return null;

        var roles   = await _userManager.GetRolesAsync(user);
        var lockout = await _userManager.IsLockedOutAsync(user);

        return new AdminUserDto
        {
            Id             = user.Id,
            FullName       = user.FullName,
            Email          = user.Email ?? string.Empty,
            PhoneNumber    = user.PhoneNumber,
            AvatarUrl      = user.AvatarUrl,
            IsActive       = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            IsLockedOut    = lockout,
            CreatedAt      = user.CreatedAt,
            LastLoginAt    = user.LastLoginAt,
            Roles          = roles.ToList(),
        };
    }
    // ─────────────────────────────────────────────
    // Create
    // ─────────────────────────────────────────────
    public async Task<Result> CreateAsync(AdminCreateUserDto dto, CancellationToken ct = default)
    {
        // 1. Check email duplicate
        var existing = await _userManager.FindByEmailAsync(dto.Email);
        if (existing is not null)
            return Result.Failure($"Email '{dto.Email}' is already registered.");
    
        // 2. Check role exists
        if (!await _roleManager.RoleExistsAsync(dto.Role))
            return Result.Failure($"Role '{dto.Role}' does not exist.");
    
        // 3. Create user
        var user = new AppUser
        {
            FullName       = dto.FullName,
            UserName       = dto.Email,
            Email          = dto.Email,
            PhoneNumber    = dto.PhoneNumber,
            EmailConfirmed = dto.EmailConfirmed,   // Admin tạo → skip email confirm
            IsActive       = dto.IsActive,
            CreatedAt      = DateTime.UtcNow,
        };
    
        var createResult = await _userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
            return Result.Failure(
                string.Join(", ", createResult.Errors.Select(e => e.Description)));
    
        // 4. Assign role
        var roleResult = await _userManager.AddToRoleAsync(user, dto.Role);
        if (!roleResult.Succeeded)
        {
            // Rollback user if role assign fails
            await _userManager.DeleteAsync(user);
            return Result.Failure(
                string.Join(", ", roleResult.Errors.Select(e => e.Description)));
        }
    
        return Result.Success();
    }
    // ─────────────────────────────────────────────
    // UPDATE
    // ─────────────────────────────────────────────
    public async Task<Result> UpdateAsync(AdminEditUserDto dto, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(dto.Id);
        if (user is null) return Result.Failure("User not found.");

        user.FullName    = dto.FullName;
        user.PhoneNumber = dto.PhoneNumber;
        user.IsActive    = dto.IsActive;
        if (user.Email == SystemAccounts.AdminEmail)
            return Result.Failure("System Admin account cannot be modified.");
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    // ─────────────────────────────────────────────
    // LOCK / UNLOCK
    // ─────────────────────────────────────────────
    public async Task<Result> LockAsync(string id, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return Result.Failure("User not found.");
        if (user.Email == SystemAccounts.AdminEmail)
            return Result.Failure("System Admin account cannot be locked.");
        await _userManager.SetLockoutEnabledAsync(user, true);
        var result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);

        return result.Succeeded ? Result.Success()
            : Result.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    public async Task<Result> UnlockAsync(string id, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return Result.Failure("User not found.");

        var result = await _userManager.SetLockoutEndDateAsync(user, null);

        return result.Succeeded ? Result.Success()
            : Result.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    // ─────────────────────────────────────────────
    // RESET PASSWORD (admin)
    // ─────────────────────────────────────────────
    public async Task<Result> ResetPasswordAsync(AdminResetPasswordDto dto, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(dto.Id);
        if (user is null) return Result.Failure("User not found.");
        if (user.Email == SystemAccounts.AdminEmail)
                return Result.Failure("System Admin password cannot be reset.");
        var token  = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);

        return result.Succeeded
            ? Result.Success()
            : Result.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    // ─────────────────────────────────────────────
    // DELETE
    // ─────────────────────────────────────────────
    public async Task<Result> DeleteAsync(string id, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return Result.Failure("User not found.");
        if (user.Email == SystemAccounts.AdminEmail)
        return Result.Failure("System Admin account cannot be deleted.");
        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    // ─────────────────────────────────────────────
    // GET ALL ROLES
    // ─────────────────────────────────────────────
    public async Task<IEnumerable<string>> GetAllRolesAsync(CancellationToken ct = default)
        => await _roleManager.Roles
            .AsNoTracking()
            .Select(r => r.Name!)
            .OrderBy(r => r)
            .ToListAsync(ct);
}