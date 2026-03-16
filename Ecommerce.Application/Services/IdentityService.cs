using Microsoft.AspNetCore.Identity;
using Ecommerce.Application.Common;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Application.Interfaces.Services;

namespace Ecommerce.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<AppUser> _userManager;

    public IdentityService(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public Task<AppUser?> FindByIdAsync(string id)
        => _userManager.FindByIdAsync(id);

    public async Task<Result<AppUser>> UpdateAsync(AppUser user)
    {
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded
            ? Result<AppUser>.Success(user)
            : Result<AppUser>.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    public async Task<Result> SetLockoutAsync(string id, bool locked)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return Result.Failure($"User #{id} not found.");

        await _userManager.SetLockoutEnabledAsync(user, true);
        await _userManager.SetLockoutEndDateAsync(user,
            locked ? DateTimeOffset.MaxValue : null);

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));
    }
}