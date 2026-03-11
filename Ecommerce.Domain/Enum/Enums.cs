using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.Domain.Enums
{
    public enum ProductStatus
    {
        Draft = 0,
        Active = 1,
        Inactive = 2,
        OutOfStock = 3,
        Discontinued = 4
    }

    public enum SortOrder
    {
        Ascending,
        Descending
    }

}