using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Application.DTOs.Admin;
using Ecommerce.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ecommerce.MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "RequireAdmin")]
    public class RolesManagerController : Controller
    {
        private readonly IRolesService _rolesService;
    
        public RolesManagerController(IRolesService rolesService)
        {
            _rolesService = rolesService;
        }
    
        // GET /RolesManager
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var roles = await _rolesService.GetAllAsync(ct);
            return View(roles);
        }
    
        // GET /RolesManager/Detail/{id}
        public async Task<IActionResult> Detail(string id, CancellationToken ct)
        {
            var role = await _rolesService.GetByIdAsync(id, ct);
            if (role is null) return NotFound();
    
            var users = await _rolesService.GetUsersInRoleAsync(role.Name, ct);
            ViewBag.Users = users;
            return View(role);
        }
    
        // POST /RolesManager/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRoleDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Role name is required.";
                return RedirectToAction(nameof(Index));
            }
    
            var result = await _rolesService.CreateAsync(dto, ct);
            TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess
                ? $"Role '{dto.Name}' created successfully."
                : result.Error;
    
            return RedirectToAction(nameof(Index));
        }
    
        // POST /RolesManager/Delete/{id}
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id, CancellationToken ct)
        {
            var result = await _rolesService.DeleteAsync(id, ct);
            TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess
                ? "Role deleted successfully."
                : result.Error;
    
            return RedirectToAction(nameof(Index));
        }
    
        // POST /RolesManager/AssignRole
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(AssignRoleDto dto, string? returnId, CancellationToken ct)
        {
            var result = await _rolesService.AssignRoleAsync(dto, ct);
            TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess
                ? $"Role '{dto.RoleName}' assigned."
                : result.Error;
    
            return !string.IsNullOrEmpty(returnId)
                ? RedirectToAction("Detail", "UsersManager", new { id = returnId })
                : RedirectToAction(nameof(Index));
        }
    
        // POST /RolesManager/RemoveRole
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveRole(AssignRoleDto dto, string? returnId, CancellationToken ct)
        {
            var result = await _rolesService.RemoveRoleAsync(dto, ct);
            TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess
                ? $"Role '{dto.RoleName}' removed."
                : result.Error;
    
            return !string.IsNullOrEmpty(returnId)
                ? RedirectToAction("Detail", "UsersManager", new { id = returnId })
                : RedirectToAction(nameof(Index));
        }
    }
}