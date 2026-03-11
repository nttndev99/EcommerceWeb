using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Application.Repositories.Interfaces;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Interfaces.Repositories
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<IEnumerable<Category>> GetActiveCategoriesAsync(CancellationToken ct = default);
        Task<Category?> GetWithChildrenAsync(int id, CancellationToken ct = default);
        Task<bool> SlugExistsAsync(string slug, int? excludeId = null, CancellationToken ct = default);
    }
}