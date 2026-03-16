using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Application.Common;

namespace Ecommerce.Application.DTOs.Admin
{
    public class UserManagerFilterParams : PaginationParams
    {
        public string? Search        { get; set; }
        public string? Role          { get; set; }
        public bool?   IsActive      { get; set; }
        public bool?   IsLockedOut   { get; set; }
        public string  SortBy        { get; set; } = "createdAt";
        public string  SortDirection { get; set; } = "desc";
    }
}