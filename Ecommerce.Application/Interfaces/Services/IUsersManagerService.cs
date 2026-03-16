using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Admin;

namespace Ecommerce.Application.Interfaces.Services
{
    public interface IUsersManagerService
    {
        Task<PagedResult<AdminUserListDto>> GetPagedAsync(UserManagerFilterParams filter, CancellationToken ct = default);
        Task<AdminUserDto?> GetByIdAsync(string id, CancellationToken ct = default);
        Task<Result> CreateAsync(AdminCreateUserDto dto, CancellationToken ct = default);   // ← NEW
        Task<Result> UpdateAsync(AdminEditUserDto dto, CancellationToken ct = default);
        Task<Result> LockAsync(string id, CancellationToken ct = default);
        Task<Result> UnlockAsync(string id, CancellationToken ct = default);
        Task<Result> ResetPasswordAsync(AdminResetPasswordDto dto, CancellationToken ct = default);
        Task<Result> DeleteAsync(string id, CancellationToken ct = default);
        Task<IEnumerable<string>> GetAllRolesAsync(CancellationToken ct = default);
    }

}