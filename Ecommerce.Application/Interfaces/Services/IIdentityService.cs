using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Application.Common;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Interfaces.Services
{
    public interface IIdentityService
    {
        Task<AppUser?> FindByIdAsync(string id);
        Task<Result<AppUser>> UpdateAsync(AppUser user);
        Task<Result> SetLockoutAsync(string id, bool locked);
    }
}