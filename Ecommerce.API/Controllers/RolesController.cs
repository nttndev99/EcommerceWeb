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
    // ROLES
    // ═══════════════════════════════════════════════════════════════
    [Authorize(Policy = "RequireAdmin")]
    public class RolesController : BaseApiController
    {
        private readonly IRolesService _rolesService;

        public RolesController(IRolesService rolesService)
            => _rolesService = rolesService;

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var roles = await _rolesService.GetAllAsync(ct);
            return Ok(roles);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateRoleDto dto, CancellationToken ct)
        {
            var result = await _rolesService.CreateAsync(dto, ct);
            return FromResult(result, "Role created.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, CancellationToken ct)
        {
            var result = await _rolesService.DeleteAsync(id, ct);
            return FromResult(result, "Role deleted.");
        }

        [HttpPost("assign")]
        public async Task<IActionResult> Assign(
            [FromBody] AssignRoleDto dto, CancellationToken ct)
        {
            var result = await _rolesService.AssignRoleAsync(dto, ct);
            return FromResult(result, "Role assigned.");
        }

        [HttpPost("remove")]
        public async Task<IActionResult> Remove(
            [FromBody] AssignRoleDto dto, CancellationToken ct)
        {
            var result = await _rolesService.RemoveRoleAsync(dto, ct);
            return FromResult(result, "Role removed.");
        }
    }
}