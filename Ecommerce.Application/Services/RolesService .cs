using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Admin;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Application.Interfaces.Services;
using static Ecommerce.Infrastructure.Identity.UsersManagerService;

namespace Ecommerce.Infrastructure.Identity;

public class RolesService : IRolesService
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<AppUser>      _userManager;

    public RolesService(RoleManager<IdentityRole> roleManager, UserManager<AppUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task<IEnumerable<RoleDto>> GetAllAsync(CancellationToken ct = default)
    {
        var roles = await _roleManager.Roles.AsNoTracking().ToListAsync(ct);
        var result = new List<RoleDto>();

        foreach (var role in roles)
        {
            var users = await _userManager.GetUsersInRoleAsync(role.Name!);
            result.Add(new RoleDto
            {
                Id        = role.Id,
                Name      = role.Name!,
                UserCount = users.Count,
            });
        }

        return result.OrderBy(r => r.Name);
    }

    public async Task<RoleDto?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role is null) return null;

        var users = await _userManager.GetUsersInRoleAsync(role.Name!);
        return new RoleDto { Id = role.Id, Name = role.Name!, UserCount = users.Count };
    }

    public async Task<IEnumerable<AdminUserListDto>> GetUsersInRoleAsync(
        string roleName, CancellationToken ct = default)
    {
        var users = await _userManager.GetUsersInRoleAsync(roleName);
        var result = new List<AdminUserListDto>();

        foreach (var u in users)
        {
            var roles    = await _userManager.GetRolesAsync(u);
            var lockout  = await _userManager.IsLockedOutAsync(u);
            result.Add(new AdminUserListDto
            {
                Id             = u.Id,
                FullName       = u.FullName,
                Email          = u.Email ?? string.Empty,
                PhoneNumber    = u.PhoneNumber,
                IsActive       = u.IsActive,
                EmailConfirmed = u.EmailConfirmed,
                IsLockedOut    = lockout,
                CreatedAt      = u.CreatedAt,
                LastLoginAt    = u.LastLoginAt,
                Roles          = roles.ToList(),
            });
        }

        return result.OrderBy(u => u.FullName);
    }

    public async Task<Result> CreateAsync(CreateRoleDto dto, CancellationToken ct = default)
    {
        if (await _roleManager.RoleExistsAsync(dto.Name))
            return Result.Failure($"Role '{dto.Name}' already exists.");

        var result = await _roleManager.CreateAsync(new IdentityRole(dto.Name));
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    public async Task<Result> DeleteAsync(string id, CancellationToken ct = default)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role is null) return Result.Failure("Role not found.");

        var users = await _userManager.GetUsersInRoleAsync(role.Name!);
        if (users.Any())
            return Result.Failure($"Cannot delete '{role.Name}' — it has {users.Count} user(s) assigned.");

        var result = await _roleManager.DeleteAsync(role);
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    public async Task<Result> AssignRoleAsync(AssignRoleDto dto, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(dto.UserId);
        if (user is null) return Result.Failure("User not found.");

        if (!await _roleManager.RoleExistsAsync(dto.RoleName))
            return Result.Failure($"Role '{dto.RoleName}' does not exist.");

        if (await _userManager.IsInRoleAsync(user, dto.RoleName))
            return Result.Failure($"User already has role '{dto.RoleName}'.");

        var result = await _userManager.AddToRoleAsync(user, dto.RoleName);
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    public async Task<Result> RemoveRoleAsync(AssignRoleDto dto, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(dto.UserId);
        if (user is null) return Result.Failure("User not found.");
        if (user.Email == SystemAccounts.AdminEmail && dto.RoleName == "Admin")
                return Result.Failure("Cannot remove Admin role from System Admin account.");
        if (!await _userManager.IsInRoleAsync(user, dto.RoleName))
            return Result.Failure($"User does not have role '{dto.RoleName}'.");

        var result = await _userManager.RemoveFromRoleAsync(user, dto.RoleName);
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));
    }
}