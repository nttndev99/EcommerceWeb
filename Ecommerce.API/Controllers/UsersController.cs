using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Application.DTOs.Admin;
using Ecommerce.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
    [ApiController]
    // ═══════════════════════════════════════════════════════════════
    // USERS MANAGER
    // ═══════════════════════════════════════════════════════════════
    [Authorize(Policy = "RequireAdmin")]
    public class UsersController : BaseApiController
    {
        private readonly IUsersManagerService _usersService;

        public UsersController(IUsersManagerService usersService)
            => _usersService = usersService;

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] UserManagerFilterParams filter, CancellationToken ct)
        {
            var result = await _usersService.GetPagedAsync(filter, ct);
            return Paged(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id, CancellationToken ct)
        {
            var user = await _usersService.GetByIdAsync(id, ct);
            return user is null ? NotFound("User not found.") : Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] AdminCreateUserDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest("Validation failed.");
            var result = await _usersService.CreateAsync(dto, ct);
            return FromResult(result, "User created.");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            string id, [FromBody] AdminEditUserDto dto, CancellationToken ct)
        {
            dto.Id = id;
            var result = await _usersService.UpdateAsync(dto, ct);
            return FromResult(result, "User updated.");
        }

        [HttpPost("{id}/lock")]
        public async Task<IActionResult> Lock(string id, CancellationToken ct)
        {
            var result = await _usersService.LockAsync(id, ct);
            return FromResult(result, "User locked.");
        }

        [HttpPost("{id}/unlock")]
        public async Task<IActionResult> Unlock(string id, CancellationToken ct)
        {
            var result = await _usersService.UnlockAsync(id, ct);
            return FromResult(result, "User unlocked.");
        }

        [HttpPost("{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(
            string id, [FromBody] AdminResetPasswordDto dto, CancellationToken ct)
        {
            dto.Id = id;
            var result = await _usersService.ResetPasswordAsync(dto, ct);
            return FromResult(result, "Password reset.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, CancellationToken ct)
        {
            var result = await _usersService.DeleteAsync(id, ct);
            return FromResult(result, "User deleted.");
        }
    }

}