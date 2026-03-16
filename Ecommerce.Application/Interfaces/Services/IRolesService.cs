using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Admin;

namespace Ecommerce.Application.Interfaces.Services
{
    public interface IRolesService
    {
        Task<IEnumerable<RoleDto>> GetAllAsync(CancellationToken ct = default);
        Task<RoleDto?> GetByIdAsync(string id, CancellationToken ct = default);
        Task<IEnumerable<AdminUserListDto>> GetUsersInRoleAsync(string roleName, CancellationToken ct = default);
        Task<Result> CreateAsync(CreateRoleDto dto, CancellationToken ct = default);
        Task<Result> DeleteAsync(string id, CancellationToken ct = default);
        Task<Result> AssignRoleAsync(AssignRoleDto dto, CancellationToken ct = default);
        Task<Result> RemoveRoleAsync(AssignRoleDto dto, CancellationToken ct = default);
    }

}